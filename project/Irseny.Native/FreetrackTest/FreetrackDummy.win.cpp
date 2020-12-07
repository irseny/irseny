
#if WITH_WINAPI
#include "windows.h"

int main(int argc, char** args) {
	while (1) {
		Sleep(16);
	}
	return 0;
}
#endif // WITH_WINAPI
