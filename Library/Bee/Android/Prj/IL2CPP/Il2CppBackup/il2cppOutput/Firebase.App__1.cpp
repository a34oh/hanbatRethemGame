#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>


template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};

struct Dictionary_2_tD81F54C87D78FE70A5DE7DAA170AE5EB4E54E8C3;
struct Dictionary_2_t070EAA8A0D7DC2B4DA1223E3809A83B3933BF21A;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct AppOptions_tC85C010A614E35ED5C64709D909D4525D9DE6D09;
struct EventHandler_tC6323FD7E6163F965259C33D72612C0E5B9BAB82;
struct FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25;
struct FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct String_t;
struct Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E;
struct UriParser_t920B0868286118827C08B08A15A9456AF6C19D81;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E;
struct UriInfo_t5F91F77A93545DDDA6BB24A609BAF5E232CC1A09;

IL2CPP_EXTERN_C RuntimeClass* FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E_il2cpp_TypeInfo_var;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;


IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct AppOptions_tC85C010A614E35ED5C64709D909D4525D9DE6D09  : public RuntimeObject
{
	Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* ___U3CDatabaseUrlU3Ek__BackingField;
	String_t* ___U3CAppIdU3Ek__BackingField;
	String_t* ___U3CApiKeyU3Ek__BackingField;
	String_t* ___U3CMessageSenderIdU3Ek__BackingField;
	String_t* ___U3CStorageBucketU3Ek__BackingField;
	String_t* ___U3CProjectIdU3Ek__BackingField;
	String_t* ___U3CPackageNameU3Ek__BackingField;
};
struct FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2  : public RuntimeObject
{
	WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* ___U3CappU3Ek__BackingField;
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct GCHandle_tC44F6F72EE68BD4CFABA24309DA7A179D41127DC 
{
	intptr_t ___handle;
};
struct HandleRef_t4B05E32B68797F702257D4E838B85A976313F08F 
{
	RuntimeObject* ____wrapper;
	intptr_t ____handle;
};
struct UriIdnScope_t001CC97A6F977E9BB7DB855CC6BA415A7F47491F 
{
	int32_t ___value__;
};
struct Flags_t47CF4DB4036A6A539AFA6EE39C75F772E865E897 
{
	uint64_t ___value__;
};
struct FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25  : public RuntimeObject
{
	HandleRef_t4B05E32B68797F702257D4E838B85A976313F08F ___swigCPtr;
	bool ___swigCMemOwn;
	String_t* ___name;
	EventHandler_tC6323FD7E6163F965259C33D72612C0E5B9BAB82* ___AppDisposed;
	FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* ___appPlatform;
};
struct SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295  : public Exception_t
{
};
struct Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E  : public RuntimeObject
{
	String_t* ___m_String;
	String_t* ___m_originalUnicodeString;
	UriParser_t920B0868286118827C08B08A15A9456AF6C19D81* ___m_Syntax;
	String_t* ___m_DnsSafeHost;
	uint64_t ___m_Flags;
	UriInfo_t5F91F77A93545DDDA6BB24A609BAF5E232CC1A09* ___m_Info;
	bool ___m_iriParsing;
};
struct WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E  : public RuntimeObject
{
	bool ___isLongReference;
	GCHandle_tC44F6F72EE68BD4CFABA24309DA7A179D41127DC ___gcHandle;
};
struct InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_StaticFields
{
	RuntimeObject* ___disposeLock;
	Dictionary_2_t070EAA8A0D7DC2B4DA1223E3809A83B3933BF21A* ___nameToProxy;
	Dictionary_2_tD81F54C87D78FE70A5DE7DAA170AE5EB4E54E8C3* ___cPtrToProxy;
	bool ___AppUtilCallbacksInitialized;
	RuntimeObject* ___AppUtilCallbacksLock;
	bool ___PreventOnAllAppsDestroyed;
	bool ___crashlyticsInitializationAttempted;
	bool ___userAgentRegistered;
	int32_t ___CheckDependenciesThread;
	RuntimeObject* ___CheckDependenciesThreadLock;
};
struct Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E_StaticFields
{
	String_t* ___UriSchemeFile;
	String_t* ___UriSchemeFtp;
	String_t* ___UriSchemeGopher;
	String_t* ___UriSchemeHttp;
	String_t* ___UriSchemeHttps;
	String_t* ___UriSchemeWs;
	String_t* ___UriSchemeWss;
	String_t* ___UriSchemeMailto;
	String_t* ___UriSchemeNews;
	String_t* ___UriSchemeNntp;
	String_t* ___UriSchemeNetTcp;
	String_t* ___UriSchemeNetPipe;
	String_t* ___SchemeDelimiter;
	bool ___s_ConfigInitialized;
	bool ___s_ConfigInitializing;
	int32_t ___s_IdnScope;
	bool ___s_IriParsing;
	bool ___useDotNetRelativeOrAbsolute;
	bool ___IsWindowsFileSystem;
	RuntimeObject* ___s_initLock;
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___HexLowerChars;
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ____WSchars;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif



IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WeakReference__ctor_m8085B7DB432EB4B11F2FFDB543B3F1D05D4A8D99 (WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* __this, RuntimeObject* ___0_target, bool ___1_trackResurrection, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void FirebaseAppPlatform_set_app_mEB3529E3DAB01F9F1BE970AA2FF0F9488AFED5D6_inline (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* ___0_value, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* FirebaseAppPlatform_get_app_m26B9904153AC094A6BEA240F74B082EEDB9C35D7_inline (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* FirebaseAppPlatform_get_AppObject_m688F6AB3978E0320042F2EB1813A985B31E211EC (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* FirebaseAppPlatform_get_App_mF79CB10DF6320F7EE7C1FEB875B8FBA5910853F2 (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* FirebaseApp_get_Name_m89C11F96726C8E4FD3CCAE04A5DC3129F7CD975E (FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR AppOptions_tC85C010A614E35ED5C64709D909D4525D9DE6D09* FirebaseApp_get_Options_mDD04F28C88747DC1332D1A9363E7B6F6FCC7A12E (FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* AppOptions_get_DatabaseUrl_mD70471C397E4022DBBF141396FED531CA8C0C8B4_inline (AppOptions_tC85C010A614E35ED5C64709D909D4525D9DE6D09* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* FirebaseAppPlatform_get_app_m26B9904153AC094A6BEA240F74B082EEDB9C35D7 (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) 
{
	{
		WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* L_0 = __this->___U3CappU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void FirebaseAppPlatform_set_app_mEB3529E3DAB01F9F1BE970AA2FF0F9488AFED5D6 (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* ___0_value, const RuntimeMethod* method) 
{
	{
		WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* L_0 = ___0_value;
		__this->___U3CappU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CappU3Ek__BackingField), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void FirebaseAppPlatform__ctor_m624B5007F157A808E31A87277CF06907370529A7 (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* ___0_wrappedApp, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* L_0 = ___0_wrappedApp;
		WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* L_1 = (WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E*)il2cpp_codegen_object_new(WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E_il2cpp_TypeInfo_var);
		WeakReference__ctor_m8085B7DB432EB4B11F2FFDB543B3F1D05D4A8D99(L_1, L_0, (bool)0, NULL);
		FirebaseAppPlatform_set_app_mEB3529E3DAB01F9F1BE970AA2FF0F9488AFED5D6_inline(__this, L_1, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* FirebaseAppPlatform_get_AppObject_m688F6AB3978E0320042F2EB1813A985B31E211EC (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) 
{
	RuntimeObject* V_0 = NULL;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
	}
	try
	{
		WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* L_0;
		L_0 = FirebaseAppPlatform_get_app_m26B9904153AC094A6BEA240F74B082EEDB9C35D7_inline(__this, NULL);
		NullCheck(L_0);
		RuntimeObject* L_1;
		L_1 = VirtualFuncInvoker0< RuntimeObject* >::Invoke(6, L_0);
		V_0 = L_1;
		goto IL_0016;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0010;
		}
		throw e;
	}

CATCH_0010:
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_2 = ((InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)IL2CPP_GET_ACTIVE_EXCEPTION(InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*));;
		V_0 = NULL;
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		goto IL_0016;
	}

IL_0016:
	{
		RuntimeObject* L_3 = V_0;
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* FirebaseAppPlatform_get_App_mF79CB10DF6320F7EE7C1FEB875B8FBA5910853F2 (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* V_0 = NULL;
	{
		RuntimeObject* L_0;
		L_0 = FirebaseAppPlatform_get_AppObject_m688F6AB3978E0320042F2EB1813A985B31E211EC(__this, NULL);
		V_0 = ((FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25*)IsInstSealed((RuntimeObject*)L_0, FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var));
		goto IL_000f;
	}

IL_000f:
	{
		FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* L_1 = V_0;
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* FirebaseAppPlatform_get_Name_mA335A35AE3D3BEA5E0FB4B8E387BB5B89731CD26 (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) 
{
	FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* V_0 = NULL;
	bool V_1 = false;
	String_t* V_2 = NULL;
	{
		FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* L_0;
		L_0 = FirebaseAppPlatform_get_App_mF79CB10DF6320F7EE7C1FEB875B8FBA5910853F2(__this, NULL);
		V_0 = L_0;
		FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* L_1 = V_0;
		V_1 = (bool)((!(((RuntimeObject*)(FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25*)L_1) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_001a;
		}
	}
	{
		FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* L_3 = V_0;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = FirebaseApp_get_Name_m89C11F96726C8E4FD3CCAE04A5DC3129F7CD975E(L_3, NULL);
		V_2 = L_4;
		goto IL_001f;
	}

IL_001a:
	{
		V_2 = (String_t*)NULL;
		goto IL_001f;
	}

IL_001f:
	{
		String_t* L_5 = V_2;
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* FirebaseAppPlatform_get_DatabaseUrl_mC8E196F8B4AC72857A73F703CBB43155B993AE00 (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) 
{
	FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* V_0 = NULL;
	bool V_1 = false;
	Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* V_2 = NULL;
	{
		FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* L_0;
		L_0 = FirebaseAppPlatform_get_App_mF79CB10DF6320F7EE7C1FEB875B8FBA5910853F2(__this, NULL);
		V_0 = L_0;
		FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* L_1 = V_0;
		V_1 = (bool)((!(((RuntimeObject*)(FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25*)L_1) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0);
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_001f;
		}
	}
	{
		FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* L_3 = V_0;
		NullCheck(L_3);
		AppOptions_tC85C010A614E35ED5C64709D909D4525D9DE6D09* L_4;
		L_4 = FirebaseApp_get_Options_mDD04F28C88747DC1332D1A9363E7B6F6FCC7A12E(L_3, NULL);
		NullCheck(L_4);
		Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* L_5;
		L_5 = AppOptions_get_DatabaseUrl_mD70471C397E4022DBBF141396FED531CA8C0C8B4_inline(L_4, NULL);
		V_2 = L_5;
		goto IL_0024;
	}

IL_001f:
	{
		V_2 = (Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E*)NULL;
		goto IL_0024;
	}

IL_0024:
	{
		Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* L_6 = V_2;
		return L_6;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void FirebaseAppPlatform_set_app_mEB3529E3DAB01F9F1BE970AA2FF0F9488AFED5D6_inline (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* ___0_value, const RuntimeMethod* method) 
{
	{
		WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* L_0 = ___0_value;
		__this->___U3CappU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CappU3Ek__BackingField), (void*)L_0);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* FirebaseAppPlatform_get_app_m26B9904153AC094A6BEA240F74B082EEDB9C35D7_inline (FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* __this, const RuntimeMethod* method) 
{
	{
		WeakReference_tD4B0518CE911FFD9FAAB3FCD492644A354312D8E* L_0 = __this->___U3CappU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* AppOptions_get_DatabaseUrl_mD70471C397E4022DBBF141396FED531CA8C0C8B4_inline (AppOptions_tC85C010A614E35ED5C64709D909D4525D9DE6D09* __this, const RuntimeMethod* method) 
{
	{
		Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* L_0 = __this->___U3CDatabaseUrlU3Ek__BackingField;
		return L_0;
	}
}
