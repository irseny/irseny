cmake_minimum_required(VERSION 2.8)

set(PROJECT_NAME Extrack)
set(FT_DUMMY_NAME TrackIR)
set(FT_TEST_NAME CheckFT)
project(${PROJECT_NAME} C)

# project paths
get_filename_component(TOOL_DIR "${CMAKE_CURRENT_SOURCE_DIR}" DIRECTORY)
get_filename_component(ROOT_DIR "${TOOL_DIR}" DIRECTORY)
set(SOLUTION_DIR "${ROOT_DIR}/project")
set(PROJECT_DIR "${SOLUTION_DIR}/Extrack")
set(PROJECT_SOURCE_DIR "${PROJECT_DIR}")
set(PROJECT_LIB_DIR "${ROOT_DIR}/lib/Extrack")

# target platform
if (UNIX)
	set(PROJECT_PLATFORM "Linux")
elseif (WIN32)
	set(PROJECT_PLATFORM "Windows")
else()
	message(WARNING "Platform not supported")
endif()

# project configuration
if (CMAKE_BUILD_TYPE MATCHES Debug)
	set(PROJECT_CONFIGURATION "${PROJECT_PLATFORM}Debug")
elseif (CMAKE_BUILD_TYPE MATCHES Release)
	set(PROJECT_CONFIGURATION "${PROJECT_PLATFORM}Release")
else()
	message(WARNING "Build type unspecified: ${CMAKE_BUILD_TYPE}")
	set(PROJECT_CONFIGURATION "Build")
endif()
set(PROJECT_BIN_DIR "${ROOT_DIR}/bin/${PROJECT_CONFIGURATION}")

# force 32 bit
if (TARGET_M32)
	list(APPEND PROJECT_COMPILE_OPTIONS "-m32")
	list(APPEND PROJECT_LINKER_FLAGS "-m32")
endif()
# os flags
if (WIN32)
	list(APPEND PROJECT_COMPILE_DEFINITIONS "WINDOWS")
elseif (UNIX)
	list(APPEND PROJECT_COMPILE_DEFINITIONS "LINUX")
endif()
# find vjoy
if (WITH_VJOY)
	list(APPEND PROJECT_COMPILE_DEFINITIONS "WITH_VJOY" "WITH_JOYSTICK")
endif()
# enable freetrack
if (WITH_FREETRACK)
	list(APPEND PROJECT_COMPILE_DEFINITIONS "WITH_FREETRACK")
endif()
# enable winapi
if (WITH_WINAPI)
	list(APPEND PROJECT_COMPILE_DEFINITIONS "WITH_WINAPI")
endif()
# enable uinput
if (WITH_UINPUT)
	list(APPEND PROJECT_COMPILE_DEFINITIONS "WITH_UINPUT")
endif()

# extrack target
# source files
if (WIN32)
	list(APPEND EXTRACK_SOURCE_FILES "${PROJECT_SOURCE_DIR}/InjectWin.c")
elseif (UNIX)
	list(APPEND EXTRACK_SOURCE_FILES "${PROJECT_SOURCE_DIR}/InjectLin.c")
endif()
# target
add_library(${PROJECT_NAME} SHARED ${EXTRACK_SOURCE_FILES})
target_include_directories(${PROJECT_NAME} PUBLIC "${PROJECT_SOURCE_DIR}")
# compiler options
target_compile_options(${PROJECT_NAME} PUBLIC "${PROJECT_COMPILE_OPTIONS}")
target_link_libraries(${PROJECT_NAME} PUBLIC "${PROJECT_LINKER_FLAGS}")
target_compile_definitions(${PROJECT_NAME} PUBLIC "${PROJECT_COMPILE_DEFINITIONS}")
# installation
install(TARGETS ${PROJECT_NAME} DESTINATION "${PROJECT_LIB_DIR}")
install(TARGETS ${PROJECT_NAME} DESTINATION "${PROJECT_BIN_DIR}")

# freetrack dummy target
if (WITH_FREETRACK_DUMMY)
	# source files
	if (WIN32)
		list(APPEND FT_DUMMY_SOURCE_FILES "${PROJECT_SOURCE_DIR}/FTDummyWin.c")
	elseif(UNIX)
		list(APPEND FT_DUMMY_SOURCE_FILES "${PROJECT_SOURCE_DIR}/FTDummyLin.c")
	endif()
	# target
	add_executable(${FT_DUMMY_NAME} ${FT_DUMMY_SOURCE_FILES})
	target_include_directories(${FT_DUMMY_NAME} PUBLIC "${PROJECT_SOURCE_DIR}")
	# compiler options
	target_compile_options(${FT_DUMMY_NAME} PUBLIC "${PROJECT_COMPILE_OPTIONS}")
	target_link_libraries(${FT_DUMMY_NAME} PUBLIC "${PROJECT_LINKER_FLAGS}")
	target_compile_definitions(${FT_DUMMY_NAME} PUBLIC "${PROJECT_COMPILE_DEFINITIONS}")
	# installation
	install(TARGETS ${FT_DUMMY_NAME} DESTINATION "${PROJECT_BIN_DIR}")
endif()

# freetrack test target
if (WITH_FREETRACK)
	# source files
	if (WIN32)
		list(APEND FT_TEST_SOURCE_FILES "${PROJECT_SOURCE_DIR}/FTTestWin.c")
	elseif(UNIX)
		list(APPEND FT_TEST_SOURCE_FILES "${PROJECT_SOURCE_DIR}/FTTestLin.c")
	endif()
	# target
	add_executable(${FT_TEST_NAME} "${FT_TEST_SOURCE_FILES}")
	target_include_directories(${FT_TEST_NAME} PUBLIC "${PROJECT_SOURCE_DIR}")
	# compiler options
	target_compile_options(${FT_TEST_NAME} PUBLIC "${PROJECT_COMPILE_OPTIONS}")
	target_link_libraries(${FT_TEST_NAME} PUBLIC "${PROJECT_LINKER_FLAGS}")
	target_compile_definitions(${FT_TEST_NAME} PUBLIC "${PROJECT_COMPILE_DEFINITIONS}")
	# installation
	install(TARGETS ${FT_TEST_NAME} DESTINATION "${PROJECT_BIN_DIR}")
endif()
