#ifdef LINUX

#include "unistd.h"

int main(int argc, char** args) {
	while (1) {
		usleep(16384);
	}
	return 0;
}
#endif // LINUX
