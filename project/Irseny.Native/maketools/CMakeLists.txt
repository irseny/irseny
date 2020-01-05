cmake_minimum_required(VERSION 2.8)

set(IRSENY_NATIVE_NAME Irseny.Native)
set(FREETRACK_DUMMY_NAME TrackIR)
set(FREETRACK_TEST_NAME CheckFT)
project(${IRSENY_NATIVE_NAME} C)

# project paths
get_filename_component(IRSENY_NATIVE_DIR "${CMAKE_CURRENT_SOURCE_DIR}" DIRECTORY)
get_filename_component(IRSENY_PROJECT_DIR "${IRSENY_NATIVE_DIR}" DIRECTORY)
get_filename_component(IRSENY_ROOT_DIR "${IRSENY_PROJECT_DIR}" DIRECTORY)

set(IRSENY_NATIVE_SOURCE_DIR "${IRSENY_NATIVE_DIR}")

# target platform
if (UNIX)
	set(IRSENY_PLATFORM "Linux")
elseif (WIN32)
	set(IRSENY_PLATFORM "Windows")
else()
	message(WARNING "Platform not supported (Could not detect Windows or Linux)")
	set(IRSENY_PLATFORM "Unknown")
endif()

# project configuration
if (CMAKE_BUILD_TYPE MATCHES Debug)
	set(IRSENY_CONFIGURATION "${IRSENY_PLATFORM}Debug")
elseif (CMAKE_BUILD_TYPE MATCHES Release)
	set(IRSENY_CONFIGURATION "${IRSENY_PLATFORM}Release")
else()
	message(WARNING "Build type unspecified: ${CMAKE_BUILD_TYPE}")
	set(IRSENY_CONFIGURATION "${IRSENY_PLATFORM}Build")
endif()
set(IRSENY_LIB_DIR "${IRSENY_ROOT_DIR}/lib")
set(IRSENY_BIN_DIR "${IRSENY_ROOT_DIR}/bin")

# force 32 bit
if (TARGET_M32)
	list(APPEND IRSENY_COMPILE_OPTIONS "-m32")
	list(APPEND IRSENY_LINKER_FLAGS "-m32")
endif()
# os flags
if (WIN32)
	list(APPEND IRSENY_COMPILE_DEFINITIONS "WINDOWS")
elseif (UNIX)
	list(APPEND IRSENY_COMPILE_DEFINITIONS "LINUX")
endif()
# find vjoy
if (WITH_VJOY)
	list(APPEND IRSENY_COMPILE_DEFINITIONS "WITH_VJOY" "WITH_JOYSTICK")
endif()
# enable freetrack
if (WITH_FREETRACK)
	list(APPEND IRSENY_COMPILE_DEFINITIONS "WITH_FREETRACK")
endif()
# enable winapi
if (WITH_WINAPI)
	list(APPEND IRSENY_COMPILE_DEFINITIONS "WITH_WINAPI")
endif()
# enable uinput
if (WITH_UINPUT)
	list(APPEND IRSENY_COMPILE_DEFINITIONS "WITH_UINPUT")
endif()

# Irseny.Native target
# source files
if (WIN32)
	list(APPEND IRSENY_NATIVE_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/Inject/InjectWin.c")
elseif (UNIX)
	list(APPEND IRSENY_NATIVE_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/Inject/InjectLin.c")
endif()
# target
add_library(${IRSENY_NATIVE_NAME} SHARED ${IRSENY_NATIVE_SOURCE_FILES})
target_include_directories(${IRSENY_NATIVE_NAME} PUBLIC "${PROJECT_SOURCE_DIR}")
# compiler options
target_compile_options(${IRSENY_NATIVE_NAME} PUBLIC "${IRSENY_COMPILE_OPTIONS}")
target_link_libraries(${IRSENY_NATIVE_NAME} PUBLIC "${IRSENY_LINKER_FLAGS}")
target_compile_definitions(${IRSENY_NATIVE_NAME} PUBLIC "${IRSENY_COMPILE_DEFINITIONS}")
# installation
install(TARGETS ${IRSENY_NATIVE_NAME} DESTINATION "${IRSENY_LIB_DIR}")
install(TARGETS ${IRSENY_NATIVE_NAME} DESTINATION "${IRSENY_BIN_DIR}")

# freetrack dummy target
if (WITH_FREETRACK_DUMMY)
	# source files
	if (WIN32)
		list(APPEND FREETRACK_DUMMY_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/FreetrackTools/DummyWin.c")
	elseif(UNIX)
		list(APPEND FREETRACK_DUMMY_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/FreetrackTools/DummyLin.c")
	endif()
	# target
	add_executable(${FREETRACK_DUMMY_NAME} ${FREETRACK_DUMMY_SOURCE_FILES})
	target_include_directories(${FREETRACK_DUMMY_NAME} PUBLIC "${IRSENY_NATIVE_SOURCE_DIR}/Inject")
	# compiler options
	target_compile_options(${FREETRACK_DUMMY_NAME} PUBLIC "${IRSENY_COMPILE_OPTIONS}")
	target_link_libraries(${FREETRACK_DUMMY_NAME} PUBLIC "${IRSENY_LINKER_FLAGS}")
	target_compile_definitions(${FREETRACK_DUMMY_NAME} PUBLIC "${IRSENY_COMPILE_DEFINITIONS}")
	# installation
	install(TARGETS ${FREETRACK_DUMMY_NAME} DESTINATION "${IRSENY_BIN_DIR}")
endif()

# freetrack test target
if (WITH_FREETRACK_TEST)
	# source files
	if (WIN32)
		list(APPEND FREETRACK_TEST_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/FreetrackTools/TestWin.c")
	elseif(UNIX)
		list(APPEND FREETRACK_TEST_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/FreetrackTools/TestLin.c")
	endif()
	# target
	add_executable(${FREETRACK_TEST_NAME} "${FREETRACK_TEST_SOURCE_FILES}")
	target_include_directories(${FREETRACK_TEST_NAME} PUBLIC "${IRSENY_NATIVE_SOURCE_DIR}/FreetrackTools")
	# compiler options
	target_compile_options(${FREETRACK_TEST_NAME} PUBLIC "${IRSENY_COMPILE_OPTIONS}")
	target_link_libraries(${FREETRACK_TEST_NAME} PUBLIC "${IRSENY_LINKER_FLAGS}")
	target_compile_definitions(${FREETRACK_TEST_NAME} PUBLIC "${IRSENY_COMPILE_DEFINITIONS}")
	# installation
	install(TARGETS ${FREETRACK_TEST_NAME} DESTINATION "${IRSENY_BIN_DIR}")
endif()
