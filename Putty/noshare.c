/*
 * Stub implementation of SSH connection-sharing IPC, for any
 * platform which can't support it at all.
 */

#include <stdio.h>
#include <assert.h>
#include <errno.h>

#include "TREE234.H"
#include "PUTTY.H"
#include "SSH.H"
#include "NETWORK.H"

int platform_ssh_share(const char *name, Conf *conf,
                       Plug downplug, Plug upplug, Socket *sock,
                       char **logtext, char **ds_err, char **us_err,
                       int can_upstream, int can_downstream)
{
    return SHARE_NONE;
}

void platform_ssh_share_cleanup(const char *name)
{
}
