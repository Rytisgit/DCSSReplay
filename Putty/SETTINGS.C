/*
 * settings.c: read and write saved sessions. (platform-independent)
 */

#include <assert.h>
#include <stdio.h>
#include <stdlib.h>
#include "PUTTY.H"
#include "storage.h"

/* The cipher order given here is the default order. */
static const struct keyvalwhere ciphernames[] = {
    { "aes",        CIPHER_AES,             -1, -1 },
    { "blowfish",   CIPHER_BLOWFISH,        -1, -1 },
    { "3des",       CIPHER_3DES,            -1, -1 },
    { "WARN",       CIPHER_WARN,            -1, -1 },
    { "arcfour",    CIPHER_ARCFOUR,         -1, -1 },
    { "des",        CIPHER_DES,             -1, -1 }
};

static const struct keyvalwhere kexnames[] = {
    { "dh-gex-sha1",        KEX_DHGEX,      -1, -1 },
    { "dh-group14-sha1",    KEX_DHGROUP14,  -1, -1 },
    { "dh-group1-sha1",     KEX_DHGROUP1,   -1, -1 },
    { "rsa",                KEX_RSA,        KEX_WARN, -1 },
    { "WARN",               KEX_WARN,       -1, -1 }
};

/*
 * All the terminal modes that we know about for the "TerminalModes"
 * setting. (Also used by config.c for the drop-down list.)
 * This is currently precisely the same as the set in ssh.c, but could
 * in principle differ if other backends started to support tty modes
 * (e.g., the pty backend).
 */
const char *const ttymodes[] = {
    "INTR",	"QUIT",     "ERASE",	"KILL",     "EOF",
    "EOL",	"EOL2",     "START",	"STOP",     "SUSP",
    "DSUSP",	"REPRINT",  "WERASE",	"LNEXT",    "FLUSH",
    "SWTCH",	"STATUS",   "DISCARD",	"IGNPAR",   "PARMRK",
    "INPCK",	"ISTRIP",   "INLCR",	"IGNCR",    "ICRNL",
    "IUCLC",	"IXON",     "IXANY",	"IXOFF",    "IMAXBEL",
    "ISIG",	"ICANON",   "XCASE",	"ECHO",     "ECHOE",
    "ECHOK",	"ECHONL",   "NOFLSH",	"TOSTOP",   "IEXTEN",
    "ECHOCTL",	"ECHOKE",   "PENDIN",	"OPOST",    "OLCUC",
    "ONLCR",	"OCRNL",    "ONOCR",	"ONLRET",   "CS7",
    "CS8",	"PARENB",   "PARODD",	NULL
};

/*
 * Convenience functions to access the backends[] array
 * (which is only present in tools that manage settings).
 */

char *get_remote_username(Conf *conf)
{
    char *username = conf_get_str(conf, CONF_username);
    if (*username) {
	return dupstr(username);
    } else if (conf_get_int(conf, CONF_username_from_env)) {
	/* Use local username. */
	return "asdf";     /* might still be NULL */
    } else {
	return NULL;
    }
}

static char *gpps_raw(void *handle, const char *name, const char *def)
{
    char *ret = read_setting_s(handle, name);
    if (!ret)
	ret = def ? dupstr(def) : NULL;   /* permit NULL as final fallback */
    return ret;
}

static void gpps(void *handle, const char *name, const char *def,
		 Conf *conf, int primary)
{
    char *val = gpps_raw(handle, name, def);
    conf_set_str(conf, primary, val);
    sfree(val);
}

/*
 * gppfont and gppfile cannot have local defaults, since the very
 * format of a Filename or FontSpec is platform-dependent. So the
 * platform-dependent functions MUST return some sort of value.
 */


static int gppi_raw(void *handle, char *name, int def)
{
    return read_setting_i(handle, name, def);
}

static void gppi(void *handle, char *name, int def, Conf *conf, int primary)
{
    conf_set_int(conf, primary, gppi_raw(handle, name, def));
}

/*
 * Read a set of name-value pairs in the format we occasionally use:
 *   NAME\tVALUE\0NAME\tVALUE\0\0 in memory
 *   NAME=VALUE,NAME=VALUE, in storage
 * If there's no "=VALUE" (e.g. just NAME,NAME,NAME) then those keys
 * are mapped to the empty string.
 */
static int gppmap(void *handle, char *name, Conf *conf, int primary)
{
    char *buf, *p, *q, *key, *val;

    /*
     * Start by clearing any existing subkeys of this key from conf.
     */
    while ((key = conf_get_str_nthstrkey(conf, primary, 0)) != NULL)
        conf_del_str_str(conf, primary, key);

    /*
     * Now read a serialised list from the settings and unmarshal it
     * into its components.
     */
    buf = gpps_raw(handle, name, NULL);
    if (!buf)
	return FALSE;

    p = buf;
    while (*p) {
	q = buf;
	val = NULL;
	while (*p && *p != ',') {
	    int c = *p++;
	    if (c == '=')
		c = '\0';
	    if (c == '\\')
		c = *p++;
	    *q++ = c;
	    if (!c)
		val = q;
	}
	if (*p == ',')
	    p++;
	if (!val)
	    val = q;
	*q = '\0';

        if (primary == CONF_portfwd && strchr(buf, 'D') != NULL) {
            /*
             * Backwards-compatibility hack: dynamic forwardings are
             * indexed in the data store as a third type letter in the
             * key, 'D' alongside 'L' and 'R' - but really, they
             * should be filed under 'L' with a special _value_,
             * because local and dynamic forwardings both involve
             * _listening_ on a local port, and are hence mutually
             * exclusive on the same port number. So here we translate
             * the legacy storage format into the sensible internal
             * form, by finding the D and turning it into a L.
             */
            char *newkey = dupstr(buf);
            *strchr(newkey, 'D') = 'L';
            conf_set_str_str(conf, primary, newkey, "D");
            sfree(newkey);
        } else {
            conf_set_str_str(conf, primary, buf, val);
        }
    }
    sfree(buf);

    return TRUE;
}

/*
 * Write a set of name/value pairs in the above format, or just the
 * names if include_values is FALSE.
 */
static void wmap(void *handle, char const *outkey, Conf *conf, int primary,
                 int include_values)
{
    char *buf, *p, *q, *key, *realkey, *val;
    int len;

    len = 1;			       /* allow for NUL */

    for (val = conf_get_str_strs(conf, primary, NULL, &key);
	 val != NULL;
	 val = conf_get_str_strs(conf, primary, key, &key))
	len += 2 + 2 * (strlen(key) + strlen(val));   /* allow for escaping */

    buf = snewn(len, char);
    p = buf;

    for (val = conf_get_str_strs(conf, primary, NULL, &key);
	 val != NULL;
	 val = conf_get_str_strs(conf, primary, key, &key)) {

        if (primary == CONF_portfwd && !strcmp(val, "D")) {
            /*
             * Backwards-compatibility hack, as above: translate from
             * the sensible internal representation of dynamic
             * forwardings (key "L<port>", value "D") to the
             * conceptually incoherent legacy storage format (key
             * "D<port>", value empty).
             */
            char *L;

            realkey = key;             /* restore it at end of loop */
            val = "";
            key = dupstr(key);
            L = strchr(key, 'L');
            if (L) *L = 'D';
        } else {
            realkey = NULL;
        }

	if (p != buf)
	    *p++ = ',';
	for (q = key; *q; q++) {
	    if (*q == '=' || *q == ',' || *q == '\\')
		*p++ = '\\';
	    *p++ = *q;
	}
        if (include_values) {
            *p++ = '=';
            for (q = val; *q; q++) {
                if (*q == '=' || *q == ',' || *q == '\\')
                    *p++ = '\\';
                *p++ = *q;
            }
        }

        if (realkey) {
            free(key);
            key = realkey;
        }
    }
    *p = '\0';
    write_setting_s(handle, outkey, buf);
    sfree(buf);
}

static int key2val(const struct keyvalwhere *mapping,
                   int nmaps, char *key)
{
    int i;
    for (i = 0; i < nmaps; i++)
	if (!strcmp(mapping[i].s, key)) return mapping[i].v;
    return -1;
}

static const char *val2key(const struct keyvalwhere *mapping,
                           int nmaps, int val)
{
    int i;
    for (i = 0; i < nmaps; i++)
	if (mapping[i].v == val) return mapping[i].s;
    return NULL;
}

/*
 * Helper function to parse a comma-separated list of strings into
 * a preference list array of values. Any missing values are added
 * to the end and duplicates are weeded.
 * XXX: assumes vals in 'mapping' are small +ve integers
 */
static void gprefs(void *sesskey, char *name, char *def,
		   const struct keyvalwhere *mapping, int nvals,
		   Conf *conf, int primary)
{
    char *commalist;
    char *p, *q;
    int i, j, n, v, pos;
    unsigned long seen = 0;	       /* bitmap for weeding dups etc */

    /*
     * Fetch the string which we'll parse as a comma-separated list.
     */
    commalist = gpps_raw(sesskey, name, def);

    /*
     * Go through that list and convert it into values.
     */
    n = 0;
    p = commalist;
    while (1) {
        while (*p && *p == ',') p++;
        if (!*p)
            break;                     /* no more words */

        q = p;
        while (*p && *p != ',') p++;
        if (*p) *p++ = '\0';

        v = key2val(mapping, nvals, q);
        if (v != -1 && !(seen & (1 << v))) {
	    seen |= (1 << v);
            conf_set_int_int(conf, primary, n, v);
            n++;
	}
    }

    sfree(commalist);

    /*
     * Now go through 'mapping' and add values that weren't mentioned
     * in the list we fetched. We may have to loop over it multiple
     * times so that we add values before other values whose default
     * positions depend on them.
     */
    while (n < nvals) {
        for (i = 0; i < nvals; i++) {
	    assert(mapping[i].v < 32);

	    if (!(seen & (1 << mapping[i].v))) {
                /*
                 * This element needs adding. But can we add it yet?
                 */
                if (mapping[i].vrel != -1 && !(seen & (1 << mapping[i].vrel)))
                    continue;          /* nope */

                /*
                 * OK, we can work out where to add this element, so
                 * do so.
                 */
                if (mapping[i].vrel == -1) {
                    pos = (mapping[i].where < 0 ? n : 0);
                } else {
                    for (j = 0; j < n; j++)
                        if (conf_get_int_int(conf, primary, j) ==
                            mapping[i].vrel)
                            break;
                    assert(j < n);     /* implied by (seen & (1<<vrel)) */
                    pos = (mapping[i].where < 0 ? j : j+1);
                }

                /*
                 * And add it.
                 */
                for (j = n-1; j >= pos; j--)
                    conf_set_int_int(conf, primary, j+1,
                                     conf_get_int_int(conf, primary, j));
                conf_set_int_int(conf, primary, pos, mapping[i].v);
                n++;
            }
        }
    }
}

/* 
 * Write out a preference list.
 */
static void wprefs(void *sesskey, char *name,
		   const struct keyvalwhere *mapping, int nvals,
		   Conf *conf, int primary)
{
    char *buf, *p;
    int i, maxlen;

    for (maxlen = i = 0; i < nvals; i++) {
	const char *s = val2key(mapping, nvals,
                                conf_get_int_int(conf, primary, i));
	if (s) {
            maxlen += (maxlen > 0 ? 1 : 0) + strlen(s);
        }
    }

    buf = snewn(maxlen + 1, char);
    p = buf;

    for (i = 0; i < nvals; i++) {
	const char *s = val2key(mapping, nvals,
                                conf_get_int_int(conf, primary, i));
	if (s) {
            p += sprintf(p, "%s%s", (p > buf ? "," : ""), s);
	}
    }

    assert(p - buf == maxlen);
    *p = '\0';

    write_setting_s(sesskey, name, buf);

    sfree(buf);
}


static int sessioncmp(const void *av, const void *bv)
{
    const char *a = *(const char *const *) av;
    const char *b = *(const char *const *) bv;

    /*
     * Alphabetical order, except that "Default Settings" is a
     * special case and comes first.
     */
    if (!strcmp(a, "Default Settings"))
	return -1;		       /* a comes first */
    if (!strcmp(b, "Default Settings"))
	return +1;		       /* b comes first */
    /*
     * FIXME: perhaps we should ignore the first & in determining
     * sort order.
     */
    return strcmp(a, b);	       /* otherwise, compare normally */
}
