#ifndef IRSENY_EXPORT

#if LINUX
#define IRSENY_EXPORT extern "C"
#endif

#if WINDOWS
//#define IRSENY_EXPORT __attribute__(stdcall)
#define IRSENY_EXPORT __declspec(dllexport)
#endif

#endif // IRSENY_EXPORT

