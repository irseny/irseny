
# -----------------
# This script works with the existing OpenCVConfig.cmake from your specific Windows OpenCV build
# and tries to circumvent unconsidered shortcomings
# These include:
# - incompatibility with newer MSVC versions
# - not providing the full name and path to OpenCV libraries
# - not handing out runtime libraries on Windows platforms
#
# This script does not handle:
# - finding debug libraries

# This script can be configured with these variables:
# OpenCV_FIND_RUNTIME_LIBS - also search for runtime libraries AKA .dlls on Windows platforms
# OpenCV_FIND_QUIETLY - suppress all unavoidable output
# OpenCV_ARCH - processor architecture override, e.g. x64
# OpenCV_RUNTIME - msvc runtime override, e.g. vc14
# OpenCV_STATIC - whether to search for static libraries

# This script alongside OpenCVConfig.cmake sets these variables:
# OpenCV_FOUND - overall search success indicator
# OpenCV_ROOT_DIR - directory that contains OpenCVConfig.cmake
# OpenCV_LIBS - found link or runtime libraries
# OpenCV_LINK_LIBS - found link libraries (if not searching for static libs)
# OpenCV_STATIC_LIBS - found static libraries (if searching for static libs)
# OpenCV_RUNTIME_LIBS - found runtime libs (if searching for runtime libraries on the supported platforms)
# OpenCV_LIB_DIR - directory containing OpenCV libraries
# OpenCV_LINK_LIB_DIR - directory containsing OpenCV link libraries
# OpenCV_RUNTIME_LIB_DIR - directory containing OpenCV runtime libraries
# OpenCV_STATIC_LIB_DIR - directory containing static OpenCV libraries
# OpenCV_INCLUDE_DIR - directories with OpenCV headers

# OpenCV_VERSION
# OpenCV_VERSION_MAJOR
# OpenCV_VERSION_MINOR
# OpenCV_VERSION_PATCH
# OpenCV_VERSION_TWEAK
# --------------------

include(FindPackageHandleStandardArgs)

# Compares a given compiler version with an OpenCV runtime identifier to check compatibility.
# A positive result denotes compatibility while a negative result indicates imcompatibility
# of the compiler with the OpenCV build.
# MSVC_VERSION: compiler version, e.g. 1900
# OPENCV_RUNTIME: OpenCV runtime indentifier, e.g. vc14
# DIFFERENCE: comparison result
function(_opencv_compare_msvc_runtime
	MSVC_VERSION
	OPENCV_RUNTIME
	DIFFERENCE)
	
	# determine the minimum required msvc version of the given runtime
	# go through a couple of known values
	if (OPENCV_RUNTIME MATCHES "^vc[0-9]+$")
		string(SUBSTRING "${OPENCV_RUNTIME}" 2 -1 OPENCV_RUNTIME_ID)
		if (OPENCV_RUNTIME_ID LESS 8)
			# older than vc++ 8.0 is only seen in OpenCV older than 2.4
			# hence this case is not really relevant
			set(MSVC_VERSION_MIN 1400)
		elseif (OPENCV_RUNTIME_ID EQUAL 8)
			set(MSVC_VERSION_MIN 1400)
		elseif (OPENCV_RUNTIME_ID EQUAL 9)
			set(MSVC_VERSION_MIN 1500)
		elseif (OPENCV_RUNTIME_ID EQUAL 10)
			set(MSVC_VERSION_MIN 1600)
		elseif (OPENCV_RUNTIME_ID EQUAL 11)
			set(MSVC_VERSION_MIN 1700)
		elseif (OPENCV_RUNTIME_ID EQUAL 12)
			set(MSVC_VERSION_MIN 1800)
		elseif (OPENCV_RUNTIME_ID EQUAL 13)
			# vc++13 was skipped
			set(MSVC_VERSION_MIN 1900)
		elseif (OPENCV_RUNTIME_ID EQUAL 14)
			set(MSVC_VERSION_MIN 1900)
		elseif (OPENCV_RUNTIME_ID EQUAL 15)
			set(MSVC_VERSION_MIN 1910)
		elseif (OPENCV_RUNTIME_ID EQUAL 16)
			set(MSVC_VERSION_MIN 1920)
		elseif (OPENCV_RUNTIME_ID LESS 16)
			# since we went through all known values this case should not occur
			set(MSVC_VERSION_MIN 1920)
		elseif (OPENCV_RUNTIME_ID GREATER 16)
			# how do future versions look like?
			# it would be better to let OpenCVConfig handle such cases
		endif()
	else()
		# unknown runtime formats are not supported
	endif()
	if (NOT DEFINED MSVC_VERSION_MIN)
		# better let OpenCVConfig decide what to do if the input looks weird
		set(${DIFFERENCE} -1000 PARENT_SCOPE)
		return()
	endif()
	
	math(EXPR SUB_DIFFERENCE "${MSVC_VERSION} - ${MSVC_VERSION_MIN}")
	if (NOT DEFINED SUB_DIFFERENCE)
		# stop at failed calculations
		set(${DIFFERENCE} -1000 PARENT_SCOPE)
		return()
	endif()
	# otherwise a positive value indicates that msvc should be compatible
	# with the OpenCV build
	# a negative value indicates that msvc is of insufficient currency
	set(${DIFFERENCE} "${SUB_DIFFERENCE}" PARENT_SCOPE)
endfunction()


# Sets TARGET_ARCH and TARGET_RUNTIME based on the contents of ROOT_DIR 
# so that OpenCVConfig.cmake finds the OpenCV libraries
# This function replaces the in certain cases broken architecture and runtime detection
# in OpenCVConfig.cmake
# ROOT_DIR: directory where OpenCVConfig.cmake is located
# TARGET_ARCH: architecture output and user defined OpenCV_ARCH input, e.g. x64
# TARGET_RUNTIME: runtime output and user defined OpenCV_ARCH input, e.g. vc14
# MSVC_VERSION: version of the compiler e.g. 1900
function(_opencv_find_arch_runtime 
	ROOT_DIR MSVC_VERSION 
	TARGET_ARCH TARGET_RUNTIME)
	
	set(USER_ARCH ${${TARGET_ARCH}})
	set(USER_RUNTIME ${${TARGET_RUNTIME}})
	if (DEFINED USER_ARCH AND DEFINED USER_RUNTIME)
		# the user has already specified everything -> nothing to do
		return()
	endif()
	
	# first we determine the folder under ROOT_DIR 
	# through architecture indicators
	# this mirrors what is going on in OpenCVConfig
	if (DEFINED USER_ARCH)
		# already user defined
		set(ARCH_DIR "${USER_ARCH}")
	elseif("${CMAKE_GENERATOR}" MATCHES "(Win64|IA64)")
		set(ARCH_DIR "x64")
	elseif("${CMAKE_GENERATOR_PLATFORM}" MATCHES "ARM64")
		set(ARCH_DIR "ARM64")
	elseif("${CMAKE_GENERATOR}" MATCHES "ARM")
		set(ARCH_DIR "ARM")
	elseif("${CMAKE_SIZEOF_VOID_P}" STREQUAL "8")
		if (NOT OpenCV_FIND_QUIETLY)
			#message(WARNING "Cannot determine exact processor architecture for OpenCV. #Defaulting to x64")
		endif()
		set(ARCH_DIR "x64")
	elseif("${CMAKE_SIZEOF_VOID_P}" STREQUAL "4")
		if (NOT OpenCV_FIND_QUIETLY)
			#message(WARNING "Cannot determine exact processor architecture for OpenCV. #Defaulting to x86")
		endif()
		set(ARCH_DIR "x86")
	else()
		if (NOT OpenCV_FIND_QUIETLY)
			message(WARNING "Cannot determine processor architecture for OpenCV. Defaulting to x86")
		endif()
		set(ARCH_DIR "x86")
	endif()
	# then we look up whether that folder exists
	# and exit otherwise 
	if (EXISTS "${ROOT_DIR}/${ARCH_DIR}")
		if (NOT DEFINED USER_ARCH)
			set(${TARGET_ARCH} "${ARCH_DIR}" PARENT_SCOPE)
		endif()
	else()
		if (NOT OpenCV_FIND_QUIETLY)
			message(WARNING "Processor architecture ${ARCH_DIR} is not supported by your OpenCV build. ${ROOT_DIR}/${ARCH_DIR} does not exist.")
		endif()
		# ARCH_DIR stay undefined or user-defined
		return()
	endif()
	
	if (DEFINED USER_RUNTIME)
		return()
	endif()
	# second step is to look up the supported runtimes and choose the best fitting one
	# the supported runtimes are specified 
	# by the subdirectories of the architecture folder (ROOT_DIR/ARCH_DIR)
	file(GLOB AVAILABLE_RUNTIME_DIRS 
			LIST_DIRECTORIES true 
			RELATIVE "${ROOT_DIR}/${ARCH_DIR}"
			"${ROOT_DIR}/${ARCH_DIR}/*")
	# go through the results and find the runtime that best fits the given MSVC_VERSION
	set(CLOSEST_RUNTIME_DIR)
	set(CLOSEST_RUNTIME_DIFFERENCE 100000)
	foreach (_RT ${AVAILABLE_RUNTIME_DIRS})
		_opencv_compare_msvc_runtime("${MSVC_VERSION}" "${_RT}" _RT_DIFFERENCE)
		if (_RT_DIFFERENCE LESS 0)
			# not compatible -> ignore
		elseif (_RT_DIFFERENCE LESS CLOSEST_RUNTIME_DIFFERENCE)
			# choose the temporarily best fitting runtime
			set(CLOSEST_RUNTIME_DIR "${_RT}")
			set(CLOSEST_RUNTIME_DIFFERENCE  "${_RT_DIFFERENCE}")
		endif()
	endforeach()
	if (DEFINED CLOSEST_RUNTIME_DIR)
		set(${TARGET_RUNTIME} "${CLOSEST_RUNTIME_DIR}" PARENT_SCOPE)
	else()
		if (NOT DEFINED OpenCV_FIND_QUIETLY)
			message(WARNING "Cannot determine fitting OpenCV runtime from the available: ${AVAILABLE_RUNTIME_DIRS}")
		endif()
		# TARGET_RUNTIME stays undefined
	endif()
endfunction()

# initilize output variables
set(OpenCV_FOUND)
set(OpenCV_LIBS)
set(OpenCV_LINK_LIBS)
set(OpenCV_RUNTIME_LIBS)
set(OpenCV_STATIC_LIBS)
set(OpenCV_LIB_DIR)
set(OpenCV_LINK_LIB_DIR)
set(OpenCV_RUNTIME_LIB_DIR)
set(OpenCV_STATIC_LIB_DIR)
set(OpenCV_INCLUDE_DIR)

set(OpenCV_VERSION)
set(OpenCV_VERSION_MAJOR)
set(OpenCV_VERSION_MINOR)
set(OpenCV_VERSION_PATCH)
set(OpenCV_VERSION_TWEAK)

# this script relies on a configuration file within the search path
find_path(OpenCV_ROOT_DIR "OpenCVConfig.cmake")
if (NOT OpenCV_ROOT_DIR
	OR NOT EXISTS "${OpenCV_ROOT_DIR}"
	OR NOT EXISTS "${OpenCV_ROOT_DIR}/OpenCVConfig.cmake")
	
	# OpenCVConfig does not exist
	# make the operation fail and return
	set(OpenCV_ROOT_DIR)
	find_package_handle_standard_args(OpenCV
		FOUND_VAR OpenCV_FOUND
		REQUIRED_VARS OpenCV_ROOT_DIR
		FAIL_MESSAGE "OpenCVConfig.cmake not found. Add its containing folder to CMAKE_PREFIX_PATH")
	return()
endif()

# before including the config file we want to set OpenCV_ARCH and OpenCV_RUNTIME
# the auto detection in older OpenCV versions does not work property with a newer msvc
# so we try look into the folder structure and find out which OpenCV_ARCH and OpenCV_RUNTIME values would be appropriate
if (DEFINED OpenCV_ARCH AND DEFINED OpenCV_RUNTIME)
	# skip if the user has already set target values
	# OpenCVConfig will use these instead of an automatic detection
elseif (MSVC)
	#set(OpenCV_ARCH "x86")
	#set(OpenCV_RUNTIME "vc17")
	# find architecture and runtime that are compatible with the OpenCV build
	_opencv_find_arch_runtime(
		"${OpenCV_ROOT_DIR}"
		"${MSVC_VERSION}"
		OpenCV_ARCH
		OpenCV_RUNTIME)
endif()

# setup is finished
# run the detection procedure through OpenCVConfig.cmake
# after nulling all important variables

include("${OpenCV_ROOT_DIR}/OpenCVConfig.cmake")

# the lib dir does sometimes contain mulitple entries
# try to retain the first entry
if (OpenCV_LIB_DIR)
	set(_OpenCV_LIB_DIR_BACKUP "${OpenCV_LIB_DIR}")
	foreach (_DIR ${_OpenCV_LIB_DIR_BACKUP})
		set(OpenCV_LIB_DIR "${_DIR}")
		break()
	endforeach()
endif()
# consider failure conditions and output information
# must have complete platform definition
if ((NOT OpenCV_ARCH AND OpenCV_RUNTIME) 
	OR (OpenCV_ARCH AND NOT OpenCV_RUNTIME))
	
	set(OpenCV_COMPLETE)
	find_package_handle_standard_args(OpenCV
		FOUND_VAR OpenCV_FOUND
		REQUIRED_VARS OpenCV_COMPLETE OpenCV_ARCH OpenCV_RUNTIME
		FAIL_MESSAGE "Could not identify the the binary packages for your architecture and version of msvc")
	return()
endif()

# cannot continue without a known lib dir
if (NOT OpenCV_LIB_DIR)
	
	set(OpenCV_COMPLETE)
	find_package_handle_standard_args(OpenCV
		FOUND_VAR OpenCV_FOUND
		REQUIRED_VARS OpenCV_COMPLETE OpenCV_LIB_DIR
		FAIL_MESSAGE "OpenCV library path not found")
	return()
endif()

# cannot continue if the version is not specified
if (NOT OpenCV_VERSION_MAJOR)
	set(OpenCV_COMPLETE)
	find_package_handle_standard_args(OpenCV
		FOUND_VAR OpenCV_FOUND
		REQUIRED_VARS OpenCV_COMPLETE OpenCV_VERSION_MAJOR
		FAIL_MESSAGE "Misconfigured OpenCV version ${OpenCV_VERSION_MAJOR}${OpenCV_VERSION_MINOR}${OpenCV_VERSION_PATCH}${OpenCV_VERSION_TWEAK}")
	return()
endif()

# other critical error
if (NOT OpenCV_FOUND)
	set(OpenCV_COMPLETE)
	find_package_handle_standard_args(OpenCV
		FOUND_VAR OpenCV_FOUND
		REQUIRED_VARS OpenCV_COMPLETE
		FAIL_MESSAGE "OpenCV not found")
	return()
endif()

# postprocess the results
# first generate search path prefixes and suffixes
set(_OpenCV_VERSION_SUFFIX "${OpenCV_VERSION_MAJOR}${OpenCV_VERSION_MINOR}${OpenCV_VERSION_PATCH}")
get_filename_component(_OpenCV_RUNTIME_DIR "${OpenCV_LIB_DIR}" DIRECTORY)
# standard library folders
# not all OpenCV distributions necessarily come with all folders
if (NOT OpenCV_STATIC)
	set(OpenCV_LINK_LIB_DIR "${OpenCV_LIB_DIR}")
elseif (EXISTS "${_OpenCV_RUNTIME_DIR}/lib")
	set(OpenCV_LINK_LIB_DIR "${_OpenCV_RUNTIME_DIR}/lib")
endif()

if (OpenCV_STATIC)
	set(OpenCV_STATIC_LIB_DIR "${OpenCV_LIB_DIR}")
elseif (EXISTS "${_OpenCV_RUNTIME_DIR}/staticlib")
	set(OpenCV_STATIC_LIB_DIR "${_OpenCV_RUNTIME_DIR}/staticlib")
endif()

if (EXISTS "${_OpenCV_RUNTIME_DIR}/bin")
	set(OpenCV_RUNTIME_LIB_DIR "${_OpenCV_RUNTIME_DIR}/bin")
endif()

# setup some more configuration variables
set(_OpenCV_FIND_RUNTIME_LIBS ${OpenCV_FIND_RUNTIME_LIBS} AND ${MSVC} AND NOT ${OpenCV_STATIC})
set(_OpenCV_FIND_STATIC_LIBS ${OpenCV_STATIC})
set(_OpenCV_FIND_LINK_LIBS NOT ${OpenCV_STATIC})
set(OpenCV_LINK_LIBS)
set(OpenCV_RUNTIME_LIBS)
set(OpenCV_STATIC_LIBS)
set(_OpenCV_REQUIRED_LIB_VARS)

# search for library files in the previously determined library folders
# search for link libs
if (_OpenCV_FIND_LINK_LIBS)
	set(_OpenCV_MISSING_LINK_LIBS)
	foreach (_LIB ${OpenCV_LIBS})
		list(APPEND _OpenCV_REQUIRED_LIB_VARS "OpenCV_${_LIB}_LINK_LIB")
		# most OpenCVConfig files only output library names and a main library path
		# and omit the version and optional debug suffix
		# here we try to find the full paths again
		find_library(OpenCV_${_LIB}_LINK_LIB 
			NAMES "${_LIB}" "${_LIB}${_OpenCV_VERSION_SUFFIX}"
			PATHS "${OpenCV_LINK_LIB_DIR}")
		if (OpenCV_${_LIB}_LINK_LIB)
			list(APPEND OpenCV_LINK_LIBS "${OpenCV_${_LIB}_LINK_LIB}")
		else()
			list(APPEND _OpenCV_MISSING_LINK_LIBS "${_LIB}")
		endif()
	endforeach()
	if (_OpenCV_MISSING_LINK_LIBS AND NOT OpenCV_FIND_QUIETLY)
		message(SEND_ERROR "OpenCV libraries not found: ${_OpenCV_MISSING_LINK_LIBS}")
	endif()
endif()
# search for runtime libs
if (_OpenCV_FIND_RUNTIME_LIBS)
	set(_OpenCV_MISSING_RUNTIME_LIBS)
	foreach (_LIB ${OpenCV_LIBS})
		list(APPEND _OpenCV_REQUIRED_LIB_VARS "OpenCV_${_LIB}_RUNTIME_LIB")
		# since dlls are uninteresting to the compiler find_library does not consider them
		# we only find runtime libraries with find_file
		find_file(OpenCV_${_LIB}_RUNTIME_LIB 
			NAMES "${_LIB}.dll" "${_LIB}${_OpenCV_VERSION_SUFFIX}.dll"
			PATHS "${OpenCV_RUNTIME_LIB_DIR}")
		if (OpenCV_${_LIB}_RUNTIME_LIB)
			list(APPEND OpenCV_RUNTIME_LIBS "${OpenCV_${_LIB}_RUNTIME_LIB}")
		else()
			list(APPEND _OpenCV_MISSING_RUNTIME_LIBS "${_LIB}")
		endif()
	endforeach()
	if (_OpenCV_MISSING_RUNTIME_LIBS AND NOT OpenCV_FIND_QUIETLY)
		message(SEND_ERROR "OpenCV runtime libraries not found: ${_OpenCV_MISSING_RUNTIME_LIBS}")
	endif()
endif()
# search for static libs
if (_OpenCV_FIND_STATIC_LIBS)
	set(_OpenCV_MISSING_STATIC_LIBS)
	foreach (_LIB ${OpenCV_LIBS})
		list(APPEND _OpenCV_REQUIRED_LIBS "OpenCV_${_LIB}_STATIC_LIB")
		find_library(OpenCV_${_LIB}_STATIC_LIB
			NAMES "${_LIB}" "${_LIB}${_OpenCV_VERSION_SUFFIX}"
			PATHS "${OpenCV_STATIC_LIB_DIR}")
		if (OpenCV_${_LIB}_STATIC_LIB)
			list(APPEND OpenCV_STATIC_LIBS "${OpenCV_${_LIB}_STATIC_LIB}")
		else()
			list(APPEND _OpenCV_MISSING_STATIC_LIBS "${_LIB}")
		endif()
	endforeach()
	if (_OpenCV_MISSING_STATIC_LIBS AND NOT OpenCV_FIND_QUIETLY)
		message(SEND_ERROR "OpenCV static libraries not found: ${_OpenCV_MISSING_STATIC_LIBS}")
	endif()
endif()

find_package_handle_standard_args(OpenCV
	FOUND_VAR OpenCV_FOUND
	REQUIRED_VARS OpenCV_LIBS ${_OpenCV_REQUIRED_LIB_VARS})