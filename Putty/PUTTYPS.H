/*
 * Find the platform-specific header for this platform.
 */

#ifndef PUTTY_PUTTYPS_H
#define PUTTY_PUTTYPS_H

#ifdef _WINDOWS



#elif defined(MACOSX)

#include "osx.h"

#else

#include "UNIX/UNIX.H"

#endif

#endif
