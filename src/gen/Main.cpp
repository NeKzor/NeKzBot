#pragma once
#include <string.h>

#include "Utils.hpp"

#define FCVAR_DEVELOPMENTONLY	(1<<1)
#define FCVAR_HIDDEN			(1<<4)

struct ConCommandBase {
	void* VFT;
	ConCommandBase* m_pNext;
	bool m_bRegistered;
	const char* m_pszName;
	const char* m_pszHelpString;
	int m_nFlags;
};

unsigned __stdcall Main(void* args)
{
	// Check supported games
	TCHAR temp[MAX_PATH];
	GetModuleFileName(NULL, temp, _countof(temp));
	std::string exe = std::string(temp);
	int index = exe.find_last_of("\\/");
	exe = exe.substr(index + 1, exe.length() - index - 5);

	if (exe != "portal2" && exe != "hl2")
	{ return Error("Game not supported!", "gen"); }
	
	// Signature scans
	auto pcv = Scan
	(
		"engine.dll",
		(exe == "portal2")
			? "55 8B EC 81 EC ? ? ? ? 56 C6 85 ? ? ? ? ? C6 85 ? ? ? ? ? "
			: "81 EC ? ? ? ? 56 C6 84 24 ? ? ? ? ? ",
		(exe == "portal2")
			? 249
			: 258
	);
	auto pcm = Scan
	(
		"engine.dll",
		(exe == "portal2")
			? "55 8B EC 81 EC ? ? ? ? 56 8B F0 8B 06"
			: "81 EC ? ? ? ? 56 8B F0 8B 06",
		(exe == "portal2")
			? 17
			: 14
	);
	auto cvp = Scan
	(
		"engine.dll",
		(exe == "portal2")
			? "8B 0D ? ? ? ? 8B 11 8B 42 6C 68 ? ? ? ? FF D0 8B 0D ? ? ? ?"
			: "8B 0D ? ? ? ? 8B 11 8B 42 68 68 ? ? ? ? FF D0 8B 0D ? ? ? ?",
		2
	);

	// Get console for logging dev/hidden cvars
	auto tier0 = GetModuleHandleA("tier0.dll");
	auto msgAddr = GetProcAddress(tier0, "Msg");
	auto MSG = reinterpret_cast<void(__cdecl*)(const char* pMsgFormat, ...)>(msgAddr);

	// Unlock all
	auto cvarPtr = **(void***)(cvp.Address);
	auto m_pConCommandList = (ConCommandBase*)((uintptr_t)cvarPtr + ((exe == "portal2") ? 48 : 100));
	for (auto cmd = m_pConCommandList; cmd; cmd = cmd->m_pNext)
	{
		if (cmd->m_nFlags & FCVAR_DEVELOPMENTONLY && cmd->m_nFlags & FCVAR_HIDDEN)
		{
			MSG("%s [cvar_dev_hidden]\n", cmd->m_pszName);
			cmd->m_nFlags &= ~(FCVAR_DEVELOPMENTONLY | FCVAR_HIDDEN);
		}
		else if (cmd->m_nFlags & FCVAR_DEVELOPMENTONLY)
		{
			MSG("%s [cvar_dev]\n", cmd->m_pszName);
			cmd->m_nFlags &= ~(FCVAR_DEVELOPMENTONLY);
		}
		else if (cmd->m_nFlags & FCVAR_HIDDEN)
		{
			MSG("%s [cvar_hidden]\n", cmd->m_pszName);
			cmd->m_nFlags &= ~(FCVAR_HIDDEN);
		}
	}

	MSG("[cvar_list]\n");

	// New format for console output
	const char* gen = "%s[cvar_data]%s[cvar_data]%s[cvar_data]%s[end_of_cvar]";

	auto patch1 = pcv.Found
	&&
	// Remove call to StripTabsAndReturns
	DoNothingAt
	(
		pcv.Address,
		(exe == "portal2") ? 25 : 26
	)
	&&
	// Call GetHelpText
	PatchAt
	(
		pcv.Address,
		new BYTE[4]
		{
			0x8B, 0xCF,	// mov	ecx, edi
			0xFF, 0xD0	// call	eax
		},
		4
	)
	&&
	// Replace console output format
	OverwriteStringAt
	(
		pcv.Address + ((exe == "portal2") ? 48 : 51),
		gen
	);
	
	auto patch2 = pcm.Found
	&&
	// Remove call to StripTabsAndReturns
	DoNothingAt
	(
		pcm.Address,
		(exe == "portal2") ? 25 : 26
	)
	&&
	// Call GetHelpText
	PatchAt
	(
		pcm.Address,
		new BYTE[4]
		{
			0x8B, 0xCE, // mov	ecx, esi
			0xFF, 0xD2	// call	edx
		},
		4
	)
	&&
	// Replace console output format
	OverwriteStringAt
	(
		pcm.Address + ((exe == "portal2") ? 47 : 48),
		gen
	);
	
	if (!patch1) { return Error("Failed to patch PrintCvar!", "gen"); }
	if (!patch2) { return Error("Failed to patch PrintCommand!", "gen"); }

	// Now we want to execute "cvarlist" to generate everything
	return 1;
}

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID reserved)
{
	if (reason == DLL_PROCESS_ATTACH) {
		DisableThreadLibraryCalls(module);
		CreateThread(0, 0, LPTHREAD_START_ROUTINE(Main), 0, 0, 0);
	}
	return TRUE;
}