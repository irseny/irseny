cmake_minimum_required(VERSION 2.8)

project("Irseny.Native" CXX)

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
	message(WARNING "Operating system not supported")
	set(IRSENY_PLATFORM "")
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
# find opencv
if (WITH_OPENCV OR WITH_LOCAL_OPENCV)
	list(APPEND IRSENY_COMPILE_DEFINITIONS "WITH_OPENCV")
	if (WITH_LOCAL_OPENCV)
		# use custom find package script
		# set local search paths 
		# and make sure that our FindOpenCV.cmake file is used
		list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/modules")
		list(APPEND CMAKE_PREFIX_PATH "${IRSENY_LIB_DIR}/opencv")
		set(OpenCV_FIND_RUNTIME_LIBS ON)
		find_package(OpenCV MODULE REQUIRED COMPONENTS core highgui)
	else()
		find_package(OpenCV REQUIRED COMPONENTS core highgui)
	endif()
	
	#message(DEBUG " mangled paths: ${OpenCV_USE_MANGLED_PATHS}")
	list(APPEND IRSENY_LIBS "${OpenCV_LIBS}")
	list(APPEND IRSENY_INCLUDE_DIRS "${OpenCV_INCLUDE_DIRS}")
	
	#message(DEBUG " opencv version: ${OpenCV_VERSION}|${OpenCV_VERSION_MAJOR}${OpenCV_VERSION_MINOR}${OpenCV_VERSION_PATCH}" )
	#message(DEBUG " opencv suffix: ${OpenCV_MODULES_SUFFIX}")
	#message(DEBUG " opencv libs are: ${OpenCV_LIBS}")
	#message(DEBUG " opencv lib dir: ${OpenCV_LIB_DIR}")
	#message(DEBUG " opencv components: ${OpenCV_LIB_COMPONENTS}")
	#message(DEBUG " opencv include dir is: ${OpenCV_INCLUDE_DIRS}")
	#message(DEBUG " mangled paths: ${OpenCV_USE_MANGLED_PATHS}")
endif()
# find vjoy
if (WITH_VJOY)
	list(APPEND IRSENY_COMPILE_DEFINITIONS "WITH_VJOY")
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

# Irseny.Native
# source files
if (WIN32)
	list(APPEND IRSENY_NATIVE_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/InputInjection/InputInjection.win.cpp")
	list(APPEND IRSENY_NATIVE_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/VideoCapture/VideoCapture.win.cpp")
elseif (UNIX)
	list(APPEND IRSENY_NATIVE_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/InputInjection/InputInjection.lin.cpp")
	list(APPEND IRSENY_NATIVE_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/VideoCapture/VideoCapture.lin.cpp")
endif()
# target
if (WIN32)
	set(IRSENY_NATIVE "Irseny.Native")
else()
	set(IRSENY_NATIVE "irseny_native")
endif()
set(CXX_STANDARD 11)
add_library(${IRSENY_NATIVE} SHARED ${IRSENY_NATIVE_SOURCE_FILES})
target_include_directories(${IRSENY_NATIVE} PUBLIC "${IRSENY_NATIVE_DIR}")
target_include_directories(${IRSENY_NATIVE} PUBLIC "${IRSENY_INCLUDE_DIRS}")
target_compile_options(${IRSENY_NATIVE} PUBLIC "${IRSENY_COMPILE_OPTIONS}")
target_link_libraries(${IRSENY_NATIVE} PUBLIC "${IRSENY_LINKER_FLAGS}")
target_link_libraries(${IRSENY_NATIVE} PUBLIC "${IRSENY_LIBS}")
target_compile_definitions(${IRSENY_NATIVE} PUBLIC "${IRSENY_COMPILE_DEFINITIONS}")
# installation
install(TARGETS ${IRSENY_NATIVE} DESTINATION "${IRSENY_LIB_DIR}/Irseny.Native")
install(TARGETS ${IRSENY_NATIVE} DESTINATION "${IRSENY_BIN_DIR}")
message("runtime libs: ${OpenCV_RUNTIME_LIBS}")
if ((WITH_OPENCV OR WITH_LOCAL_OPENCV) AND DEFINED OpenCV_RUNTIME_LIBS)
	foreach (_LIB ${OpenCV_RUNTIME_LIBS})
		install(FILES "${_LIB}" DESTINATION "${IRSENY_BIN_DIR}")
	endforeach()
endif()

# optional FreetrackTest
if (WITH_FREETRACK_TEST)
	# source files
	if (WIN32)
		list(APPEND FREETRACK_TEST_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/FreetrackTest/FreetrackTest.win.cpp")
	elseif(UNIX)
		list(APPEND FREETRACK_TEST_SOURCE_FILES "${IRSENY_NATIVE_SOURCE_DIR}/FreetrackTest/FreetrackTest.lin.cpp")
	endif()
	# target
	set(FREETRACK_TEST "FreetrackTest")
	add_executable(${FREETRACK_TEST} "${FREETRACK_TEST_SOURCE_FILES}")
	#target_include_directories(${FREETRACK_TEST} PUBLIC "${IRSENY_NATIVE_SOURCE_DIR}/FreetrackTest")
	# compiler options
	target_compile_options(${FREETRACK_TEST} PUBLIC "${IRSENY_COMPILE_OPTIONS}")
	target_link_libraries(${FREETRACK_TEST} PUBLIC "${IRSENY_LINKER_FLAGS}")
	target_compile_definitions(${FREETRACK_TEST} PUBLIC "${IRSENY_COMPILE_DEFINITIONS}")
	# installation
	install(TARGETS ${FREETRACK_TEST} DESTINATION "${IRSENY_BIN_DIR}")
endif()
