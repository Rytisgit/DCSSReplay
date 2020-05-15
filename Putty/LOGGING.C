/*
 * Session logging.
 */

#include <stdio.h>
#include <stdlib.h>
#include <ctype.h>

#include <time.h>
#include <assert.h>

#include "putty.h"

/* log session to file stuff ... */
struct LogContext {
    FILE *lgfp;
    enum { L_CLOSED, L_OPENING, L_OPEN, L_ERROR } state;
    bufchain queue;
    Filename *currlogfilename;
    void *frontend;
    Conf *conf;
    int logtype;		       /* cached out of conf */
};

static Filename *xlatlognam(Filename *s, char *hostname, int port,
                            struct tm *tm);

/*
 * Internal wrapper function which must be called for _all_ output
 * to the log file. It takes care of opening the log file if it
 * isn't open, buffering data if it's in the process of being
 * opened asynchronously, etc.
 */
static void logwrite(struct LogContext *ctx, void *data, int len)
{
    /*
     * In state L_CLOSED, we call logfopen, which will set the state
     * to one of L_OPENING, L_OPEN or L_ERROR. Hence we process all of
     * those three _after_ processing L_CLOSED.
     */
    if (ctx->state == L_CLOSED)
	logfopen(ctx);

    if (ctx->state == L_OPENING) {
	bufchain_add(&ctx->queue, data, len);
    } else if (ctx->state == L_OPEN) {
	assert(ctx->lgfp);
	if (fwrite(data, 1, len, ctx->lgfp) < (size_t)len) {
	    logfclose(ctx);
	    ctx->state = L_ERROR;
	    /* Log state is L_ERROR so this won't cause a loop */
	    logevent(ctx->frontend,
		     "Disabled writing session log due to error while writing");
	}
    }				       /* else L_ERROR, so ignore the write */
}

/*
 * Convenience wrapper on logwrite() which printf-formats the
 * string.
 */
static void logprintf(struct LogContext *ctx, const char *fmt, ...)
{
    va_list ap;
    char *data;

    va_start(ap, fmt);
    data = dupvprintf(fmt, ap);
    va_end(ap);

    logwrite(ctx, data, strlen(data));
    sfree(data);
}

/*
 * Log session traffic.
 */
void logtraffic(void *handle, unsigned char c, int logmode)
{
    struct LogContext *ctx = (struct LogContext *)handle;
    if (ctx->logtype > 0) {
	if (ctx->logtype == logmode)
	    logwrite(ctx, &c, 1);
    }
}

/*
 * Log an Event Log entry. Used in SSH packet logging mode; this is
 * also as convenient a place as any to put the output of Event Log
 * entries to stderr when a command-line tool is in verbose mode.
 * (In particular, this is a better place to put it than in the
 * front ends, because it only has to be done once for all
 * platforms. Platforms which don't have a meaningful stderr can
 * just avoid defining FLAG_STDERR.
 */
void log_eventlog(void *handle, const char *event)
{
    struct LogContext *ctx = (struct LogContext *)handle;
    if ((flags & FLAG_STDERR) && (flags & FLAG_VERBOSE)) {
	fprintf(stderr, "%s\n", event);
	fflush(stderr);
    }
    /* If we don't have a context yet (eg winnet.c init) then skip entirely */
    if (!ctx)
	return;
    if (ctx->logtype != LGTYP_PACKETS &&
	ctx->logtype != LGTYP_SSHRAW)
	return;
    logprintf(ctx, "Event Log: %s\r\n", event);
    logflush(ctx);
}

void *log_init(void *frontend, Conf *conf)
{
    struct LogContext *ctx = snew(struct LogContext);
    ctx->lgfp = NULL;
    ctx->state = L_CLOSED;
    ctx->frontend = frontend;
    ctx->conf = conf_copy(conf);
    ctx->logtype = conf_get_int(ctx->conf, CONF_logtype);
    ctx->currlogfilename = NULL;
    bufchain_init(&ctx->queue);
    return ctx;
}

void log_free(void *handle)
{
    struct LogContext *ctx = (struct LogContext *)handle;

    logfclose(ctx);
    bufchain_clear(&ctx->queue);
    if (ctx->currlogfilename)
        filename_free(ctx->currlogfilename);
    conf_free(ctx->conf);
    sfree(ctx);
}

void log_reconfig(void *handle, Conf *conf)
{
    struct LogContext *ctx = (struct LogContext *)handle;
    int reset_logging;

    if (!filename_equal(conf_get_filename(ctx->conf, CONF_logfilename),
			conf_get_filename(conf, CONF_logfilename)) ||
	conf_get_int(ctx->conf, CONF_logtype) !=
	conf_get_int(conf, CONF_logtype))
	reset_logging = TRUE;
    else
	reset_logging = FALSE;

    if (reset_logging)
	logfclose(ctx);

    conf_free(ctx->conf);
    ctx->conf = conf_copy(conf);

    ctx->logtype = conf_get_int(ctx->conf, CONF_logtype);

    if (reset_logging)
	logfopen(ctx);
}

/*
 * translate format codes into time/date strings
 * and insert them into log file name
 *
 * "&Y":YYYY   "&m":MM   "&d":DD   "&T":hhmmss   "&h":<hostname>   "&&":&
 */
static Filename *xlatlognam(Filename *src, char *hostname, int port,
                            struct tm *tm)
{
    char buf[32], *bufp;
    int size;
    char *buffer;
    int buflen, bufsize;
    const char *s;
    Filename *ret;

    bufsize = FILENAME_MAX;
    buffer = snewn(bufsize, char);
    buflen = 0;
    s = filename_to_str(src);

    while (*s) {
        int sanitise = FALSE;
	/* Let (bufp, len) be the string to append. */
	bufp = buf;		       /* don't usually override this */
	if (*s == '&') {
	    char c;
	    s++;
	    size = 0;
	    if (*s) switch (c = *s++, tolower((unsigned char)c)) {
	      case 'y':
		size = strftime(buf, sizeof(buf), "%Y", tm);
		break;
	      case 'm':
		size = strftime(buf, sizeof(buf), "%m", tm);
		break;
	      case 'd':
		size = strftime(buf, sizeof(buf), "%d", tm);
		break;
	      case 't':
		size = strftime(buf, sizeof(buf), "%H%M%S", tm);
		break;
	      case 'h':
		bufp = hostname;
		size = strlen(bufp);
		break;
	      case 'p':
                size = sprintf(buf, "%d", port);
		break;
	      default:
		buf[0] = '&';
		size = 1;
		if (c != '&')
		    buf[size++] = c;
	    }
            /* Never allow path separators - or any other illegal
             * filename character - to come out of any of these
             * auto-format directives. E.g. 'hostname' can contain
             * colons, if it's an IPv6 address, and colons aren't
             * legal in filenames on Windows. */
            sanitise = TRUE;
	} else {
	    buf[0] = *s++;
	    size = 1;
	}
        if (bufsize <= buflen + size) {
            bufsize = (buflen + size) * 5 / 4 + 512;
            buffer = sresize(buffer, bufsize, char);
        }
        while (size-- > 0) {
            char c = *bufp++;
            if (sanitise)
                c = filename_char_sanitise(c);
            buffer[buflen++] = c;
        }
    }
    buffer[buflen] = '\0';

    ret = filename_from_str(buffer);
    sfree(buffer);
    return ret;
}
