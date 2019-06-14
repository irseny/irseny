cmake_minimum_required(VERSION 2.8)

set(PROJECT_NAME Extrack)
project(${PROJECT_NAME} C)

# project paths
get_filename_component(TOOL_DIR "${CMAKE_CURRENT_SOURCE_DIR}" DIRECTORY)
get_filename_component(ROOT_DIR "${TOOL_DIR}" DIRECTORY)
set(SOLUTION_DIR "${ROOT_DIR}/project")
set(PROJECT_DIR "${SOLUTION_DIR}/Extrack")
set(PROJECT_SOURCE_DIR "${PROJECT_DIR}")
set(PROJECT_BIN_DIR "${ROOT_DIR}/lib/Extrack")

if (CMAKE_BUILD_TYPE MATCHES Debug)
	set(CONFIGURATION "Debug")
	if (UNIX)
		set(CONFIGURATION "LinuxDebug")
	elseif (WIN32)
		set(CONFIGURATION "WindowsDebug")
	else()
		set(CONFIGURATION "Debug")
	endif()
elseif (CMAKE_BUILD_TYPE MATCHES Release)
	if (UNIX)
		set(CONFIGURATION "LinuxRelease")
	elseif (WIN32)
		set(CONFIGURATION "WindowsRelease")
	else()
		set(CONFIGURATION "Release")
	endif()
else()
	message(WARNING "Build type unspecified: ${CMAKE_BUILD_TYPE}")
	set(CONFIGURATION "Build")
endif()
set(PROJECT_BUILD_DIR "${ROOT_DIR}/obj/${CONFIGURATION}")
include_directories("${PROJECT_SOURCE_DIR}")

# project sources
if (WIN32)
	list(APPEND PROJECT_SOURCE_FILES "${PROJECT_SOURCE_DIR}/InjectWin.c")
elseif (UNIX)
	list(APPEND PROJECT_SOURCE_FILES "${PROJECT_SOURCE_DIR}/InjectLin.c")
endif()
add_library(${PROJECT_NAME} SHARED ${PROJECT_SOURCE_FILES})

# compiler flags
if (TARGET_M32)
	target_compile_options(${PROJECT_NAME} PUBLIC -m32)
	target_link_libraries(${PROJECT_NAME} PUBLIC -m32)
endif()
# os flags
if (WIN32)
	target_compile_definitions(${PROJECT_NAME} PUBLIC "WINDOWS")
elseif (UNIX)
	target_compile_definitions(${PROJECT_NAME} PUBLIC "LINUX")
endif()
# find vjoy
if (WITH_VJOY)
	target_compile_definitions(${PROJECT_NAME} PUBLIC "WITH_VJOY")
	#target_link_libraries(${PROJECT_NAME} PUBLIC "${ROOT_DIR}/lib/VJoy")
endif()
# enable freetrack
if (WITH_FREETRACK)
	target_compile_definitions(${PROJECT_NAME} PUBLIC "WITH_FREETRACK")
endif()
# enable winapi
if (WITH_WINAPI)
	target_compile_definitions(${PROJECT_NAME} PUBLIC "WITH_WINAPI")
endif()
# enable uinput
if (WITH_UINPUT)
	target_compile_definitions(${PROJECT_NAME} PUBLIC "WITH_UINPUT")
endif()
# move library post build
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD COMMAND
	${CMAKE_COMMAND} -E copy "$<TARGET_FILE:${PROJECT_NAME}>" "${PROJECT_BIN_DIR}/$<TARGET_FILE_NAME:${PROJECT_NAME}>")
