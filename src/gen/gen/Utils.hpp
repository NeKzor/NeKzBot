#pragma once
#include <fstream>
#include <Windows.h>
#include <Psapi.h>

#define INRANGE(x, a, b) (x >= a && x <= b)
#define getBits(x) (INRANGE((x & (~0x20)), 'A', 'F') ? ((x & (~0x20)) - 'A' + 0xA): (INRANGE(x, '0', '9') ? x - '0': 0))
#define getByte(x) (getBits(x[0]) << 4 | getBits(x[1]))

struct ScanResult {
	uintptr_t Address;
	bool Found = false;
};

uintptr_t FindAddress(const uintptr_t& start_address, const uintptr_t& end_address, const char* target_pattern)
{
	const char* pattern = target_pattern;
	uintptr_t first_match = 0;

	for (uintptr_t position = start_address; position < end_address; position++) {
		if (!*pattern)
			return first_match;

		const uint8_t pattern_current = *reinterpret_cast<const uint8_t*>(pattern);
		const uint8_t memory_current = *reinterpret_cast<const uint8_t*>(position);

		if (pattern_current == '\?' || memory_current == getByte(pattern)) {
			if (!first_match)
				first_match = position;

			if (!pattern[2])
				return first_match;

			pattern += (pattern_current != '\?') ? 3 : 2;
		}
		else {
			pattern = target_pattern;
			first_match = 0;
		}
	}
	return NULL;
}

ScanResult Scan(const char* moduleName, const char* pattern, int offset = 0)
{
	auto info = MODULEINFO();
	auto result = ScanResult();

	if (GetModuleInformation(GetCurrentProcess(), GetModuleHandleA(moduleName), &info, sizeof(MODULEINFO))) {
		const uintptr_t start = uintptr_t(info.lpBaseOfDll);
		const uintptr_t end = start + info.SizeOfImage;
		result.Address = FindAddress(start, end, pattern);
		if (result.Address != NULL) {
			result.Found = true;
			result.Address += offset;
		}
	}
	return result;
}

int Error(std::string text, std::string title)
{
	MessageBoxA(0, text.c_str(), title.c_str(), MB_ICONERROR);
	return 1;
}

bool DoNothingAt(uintptr_t address, int count)
{
	BYTE nop[1] = { 0x90 };

	for (int i = 0; i < count; i++) {
		if (!WriteProcessMemory(GetCurrentProcess(), reinterpret_cast<LPVOID>(address + i), nop, 1, 0)) {
			return false;
		}
	}
	return true;
}

bool PatchAt(uintptr_t address, BYTE bytes[], int size)
{
	bool result = WriteProcessMemory(GetCurrentProcess(), reinterpret_cast<LPVOID>(address), bytes, size, 0);
	return true;
}

bool OverwriteStringAt(uintptr_t address, const char* str)
{
	bool result = WriteProcessMemory(GetCurrentProcess(), reinterpret_cast<LPVOID>(address), &str, 4, 0);
	return result;
}