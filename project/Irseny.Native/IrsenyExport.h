// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#ifndef IRSENY_EXPORT

#if LINUX
#define IRSENY_EXPORT extern "C"
#endif

#if WINDOWS
//#define IRSENY_EXPORT __attribute__(stdcall)
//#error "__declspec(dllexport)"
#define IRSENY_EXPORT extern "C" __declspec(dllexport)
#endif

#endif // IRSENY_EXPORT

