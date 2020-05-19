/* Copyright (c) 2010 Michael B. Edwin Rickert
 *
 * See the file LICENSE.txt for copying permission.
 */


#include "../../../PUTTYMEM.H"
#include "../../../TERMINAL.H"
#include "../../../PUTTY.H"


#if defined DLLEXPORTS
#if defined WIN32
#define EXPORT(rt) __declspec(dllexport) rt
#else
#define EXPORT(rt) extern rt __attribute__((visibility("default")))
#endif
#else
#if defined WIN32
#define EXPORT(rt) __declspec(dllexport) rt
#else
#define EXPORT(rt) extern rt
#endif
#endif
extern Conf *conf;

static Conf* get_default_config() {
	/*if (!conf) {
		conf = conf_new();
		load_open_settings(NULL, conf);
		conf_set_int(conf, CONF_logflush, 0);
	}
	return conf;*/
	return NULL;
}
//void CreateConsole()
//{
//	if (!AllocConsole()) {
//		// Add some error handling here.
//		// You can call GetLastError() to get more info about the error.
//		return;
//	}
//
//	// std::cout, std::clog, std::cerr, std::cin
//	FILE* fDummy;
//	freopen_s(&fDummy, "CONOUT$", "w", stdout);
//	freopen_s(&fDummy, "CONOUT$", "w", stderr);
//	freopen_s(&fDummy, "CONIN$", "r", stdin);
//
//}
EXPORT(Terminal*) CreatePuttyTerminal(int w, int h) {
	int i;
	struct unicode_data* unicode;
	Terminal* terminal;
	//CreateConsole();
	//printf("TESTSSETSETSETSET");
	unicode = snew(struct unicode_data);
	memset(unicode, 0, sizeof(struct unicode_data));

	for (i = 0; i < 256; ++i) {
		unicode->unitab_ctrl[i] = i;
		//unicode->unitab_line[i]=i;
	}

	terminal = term_init(unicode, NULL);
	term_size(terminal, h, w, 0);

	return terminal;
}

static void copy_termlines( tree234* dest, tree234* src ) {
	int w,h,x,y;
	termline *srctl, *desttl;

	h = min(count234(dest),count234(src));
	for ( y=0 ; y<h ; ++y ) {
		srctl =index234(src ,y);
		desttl=index234(dest,y);
		w = min(srctl->cols,desttl->cols);
		for ( x=0 ; x<w ; ++x ) desttl->chars[x] = srctl->chars[x];
		desttl->cc_free   = srctl->cc_free;
		desttl->cols      = srctl->cols;
		desttl->lattr     = srctl->lattr;
		desttl->size      = srctl->size;
		desttl->temporary = srctl->temporary;
	}
}

EXPORT(Terminal*) ClonePuttyTerminal( Terminal* term ) {
	struct unicode_data* unicode;
	Terminal* clone;
	Terminal  restore;

	unicode = snew(struct unicode_data);
	memcpy(unicode, term->ucsdata, sizeof(struct unicode_data));
	clone = term_init(unicode, NULL);
	term_size(clone, term->rows, term->cols, 0);
	restore = *clone;

	memcpy(clone, term, sizeof(Terminal));

	//clone->conf       = restore.conf;

	clone->screen = restore.screen;     copy_termlines(clone->screen, term->screen);
	clone->scrollback = restore.scrollback; copy_termlines(clone->scrollback, term->scrollback);
	clone->alt_screen = restore.alt_screen; copy_termlines(clone->alt_screen, term->alt_screen);

	clone->disptext = restore.disptext;
	clone->ucsdata = restore.ucsdata;

	return clone;
}

EXPORT(termchar*) GetPuttyTerminalLine(Terminal* terminal, int y) {
	termline* line = index234(terminal->screen, y);
	return line->chars;
}

EXPORT(void) DestroyPuttyTerminal(Terminal* terminal) {
	safefree(terminal->ucsdata);
	term_free(terminal);
}

EXPORT(void) SendPuttyTerminal(Terminal* terminal, int is_stderr, const char* data, int len) {
	term_data(terminal, is_stderr, data, len);
}

EXPORT(int) GetPuttyTerminalWidth(Terminal* terminal) {
	return terminal->cols;
}

EXPORT(int) GetPuttyTerminalHeight(Terminal* terminal) {
	return terminal->rows;
}
