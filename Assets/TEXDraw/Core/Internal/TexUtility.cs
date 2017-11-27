
#if (UNITY_STANDALONE || UNITY_ANDROID) && (ENABLE_MONO || ENABLE_DOTNET)
    #define EMIT // Only Standalone or Android (with Mono backend) have access to Reflection Emit
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
#if EMIT
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
#endif


namespace TexDrawLib
{
	public static class TexUtility
	{
		// Few const for simple adjusting ------------------------------------------------------------------------------
		public const float FloatPrecision = 0.001f;
        //The reason why it's 31 textures: because index 32 preserved for this block font!
        public const int blockFontIndex = 31;
        public static readonly Color white = Color.white; //Cached for speed
		public const FontStyle FontStyleDefault = (FontStyle)(-1);
		
		// Preserved Dynamic Configurations ----------------------------------------------------------------------------
		// These processed inside boxing process... all except RenderColor (that's one is used in final rendering )
		public static float 	RenderSizeFactor = 1;
		public static Color32 	RenderColor;
        public static int 		RenderFont = -1;
        public static int 		RenderTextureSize = 0;
		public static FontStyle RenderFontStyle = FontStyleDefault;
		public static float		AdditionalGlueSpace = 0;
		// RenderFont in parsing, is default -2. Which makes some problem. So sometimes we need to give a hint
        public static int 		RawRenderFont = -1;
		
            
		public static float spaceWidth
		{
			get	{ return TEXConfiguration.main.SpaceWidth; }
		}
        public static float spaceHeight
        {
            get { return TEXConfiguration.main.LineHeight; }
        }
        public static float glueRatio
		{
			get	{ return TEXConfiguration.main.GlueRatio; }
		}
		public static float lineThickness
		{
			get	{ return TEXConfiguration.main.LineThickness; }
		}

		// TexStyle manipulations (for different style, etc.) ----------------------------------------------------------
		public static float SizeFactor (TexStyle style)
		{
			if (style < TexStyle.Script)
				return RenderSizeFactor;
			else if (style < TexStyle.ScriptScript)
                return TEXConfiguration.main.ScriptFactor * RenderSizeFactor;
			else
                return TEXConfiguration.main.NestedScriptFactor * RenderSizeFactor;
		}

		public static TexStyle GetCrampedStyle (TexStyle Style)
		{
			return (int)Style % 2 == 1 ? Style : Style + 1;
		}

		public static TexStyle GetNumeratorStyle (TexStyle Style)
		{
			return Style + 2 - 2 * ((int)Style / 6);
		}

		public static TexStyle GetDenominatorStyle (TexStyle Style)
		{
			return (TexStyle)(2 * ((int)Style / 2) + 1 + 2 - 2 * ((int)Style / 6));
		}

		public static TexStyle GetRootStyle ()
		{
			return TexStyle.Script;
		}

		public static TexStyle GetSubscriptStyle (TexStyle Style)
		{
			return (TexStyle)(2 * ((int)Style / 4) + 4 + 1);
		}

		public static TexStyle GetSuperscriptStyle (TexStyle Style)
		{
			return (TexStyle)(2 * ((int)Style / 4) + 4 + ((int)Style % 2));
		}

        public static void CentreBox(Box box, TexStyle style)
        {
            float axis = TEXConfiguration.main.AxisHeight * TexUtility.SizeFactor(style);
            box.shift = (box.height - box.depth) / 2 - axis;
        }

        public static void AlignToBaseline (VerticalBox box, int baseIdx)
        {
        	float offset = 0;
        	int iter = -1;
        	baseIdx = Mathf.Min(baseIdx, box.children.Count);
			while (iter++ < baseIdx) {
				if (iter == baseIdx)
				{
					box.shift = offset;
					return;
				} 
				offset += box.children[iter].totalHeight;
			}
        }

        public static Box GetBox (Atom atom, TexStyle style)
        {
            var box = atom.CreateBox(style);
            atom.Flush();
            return box;
        }

        public static Color MultiplyColor(Color a, Color b)
        {
        	if(a == white)
        		return b;
        	return new Color (a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }

        public static Color32 MultiplyAlphaOnly(Color32 c, float a)
        {
            c.a = (byte)(c.a * a);
            return c;
        }


		 public static FontStyle TexStyle2FontStyle (FontStyle style) {
            var s = (int)style;
            switch (Mathf.Min(s, s & 3))
            {
                case -1:    case 0:    return FontStyle.Normal;
                case 1:                 return FontStyle.Bold;
                case 2:                 return FontStyle.Italic;
                case 3:                 return FontStyle.BoldAndItalic;
                default:                return FontStyle.Normal;
            }
        }

        public static string GetFontName (int idx)
        {
        	if (idx >= 0)
        		return TEXPreference.main.fontData[idx].name;
        	return "text";
        }

        // Serialize issue: In 2.x fonts are serialized by index. Now we change that by name instead (in editor)


        public static void AttemptLegacyDeserialize (ref int idx, ref string name)
        {
			try {
				var empty = string.IsNullOrEmpty(name);
				var exist = TEXPreference.main.GetFontIndexByID(name) >= 0;
	        	if (idx >= 0 && empty)
	        	{
	        		// Upgrading 2.x to 3.x
	        		name = TEXPreference.main.fontData[idx].name;
	        	}
	        	else if (exist)
	        	{
	        		// Normal deserization
	        		idx = TEXPreference.main.GetFontIndexByID(name);
	        	}
	        	else if (!exist)
	        	{
	        		// The font is not exist.
	        		// Returning index to -1 but keep the name (in case user fix it)
	        		idx = -1;
	        	}
			} catch (Exception) {
        		
			}
        }

//        public static void Attempt

    }
	
	[Serializable]
	public struct ScaleOffset
	{
        public float scale;
        public float offset;

		public ScaleOffset (float Scale, float Offset) {
            scale = Scale;
            offset = Offset;
        }
		
		public static ScaleOffset identity {
			get {
                return new ScaleOffset(1, 0);
            }
		}
        
		public float Evaluate(float v) {
            return v * scale + offset;
        }
		
		public Vector3 Evaluate (Vector3 v) {
		    return v * scale + Vector3.one * offset;
        }
    }

    [Serializable]
    public class FindReplace
    {
    	public string find;
    	public string replace;

    	[NonSerialized] Regex cachedReg;
		[NonSerialized] string cachedRegPattern;

    	public string Execute (string text, bool regex)
    	{
    		if (string.IsNullOrEmpty(find))
    			return text;

    		if (regex)
    		{
    			if (cachedRegPattern != find)
    			{
					cachedReg = new Regex(find, RegexOptions.Multiline);
					cachedRegPattern = find;
    			}
    			return cachedReg.Replace(text, replace);
    		} else
    			return text.Replace(find, replace);
    	}
    }

#if EMIT
    public delegate void MemberSetter<TTarget, TValue>(ref TTarget target, TValue value);
    public delegate TReturn MemberGetter<TTarget, TReturn>(TTarget target);
    public delegate TReturn MethodCaller<TTarget, TReturn>(TTarget target, object[] args);
    public delegate T CtorInvoker<T>(object[] parameters);

    /// <summary>
    /// Credit to vexe: https://github.com/vexe/Fast.Reflection
    /// A dynamic reflection extensions library that emits IL to set/get fields/properties, call methods and invoke constructors
    /// Once the delegate is created, it can be stored and reused resulting in much faster access times than using regular reflection
    /// The results are cached. Once a delegate is generated, any subsequent call to generate the same delegate on the same field/property/method will return the previously generated delegate
    /// Note: Since this generates IL, it won't work on AOT platforms such as iOS an Android. But is useful and works very well in editor codes and standalone targets
    /// Note: This is a trimmed version for FastReflection
    /// </summary>
    public static class FastReflection
    {
        static ILEmitter emit = new ILEmitter();
        static Dictionary<int, Delegate> cache = new Dictionary<int, Delegate>();

        const string kFieldSetterName = "FS<>";
        const string kFieldGetterName = "FG<>";
    

        /// <summary>
        /// Generates an open-instance delegate to get the value of the property from a given target
        /// </summary>
        public static MemberGetter<TTarget, TReturn> DelegateForGet<TTarget, TReturn>(this FieldInfo field)
        {
            int key = GetKey<TTarget, TReturn>(field, kFieldGetterName);
            Delegate result;
            if (cache.TryGetValue(key, out result))
                return (MemberGetter<TTarget, TReturn>)result;

            return GenDelegateForMember<MemberGetter<TTarget, TReturn>, FieldInfo>(
                field, key, kFieldGetterName, GenFieldGetter<TTarget>,
                typeof(TReturn), typeof(TTarget));
        }

        /// <summary>
        /// Generates a weakly-typed open-instance delegate to set the value of the field in a given target
        /// </summary>
        public static MemberGetter<object, object> DelegateForGet(this FieldInfo field)
        {
            return DelegateForGet<object, object>(field);
        }

        /// <summary>
        /// Generates a strongly-typed open-instance delegate to set the value of the field in a given target
        /// </summary>
        public static MemberSetter<TTarget, TValue> DelegateForSet<TTarget, TValue>(this FieldInfo field)
        {
            int key = GetKey<TTarget, TValue>(field, kFieldSetterName);
            Delegate result;
            if (cache.TryGetValue(key, out result))
                return (MemberSetter<TTarget, TValue>)result;

            return GenDelegateForMember<MemberSetter<TTarget, TValue>, FieldInfo>(
                field, key, kFieldSetterName, GenFieldSetter<TTarget>,
                typeof(void), typeof(TTarget).MakeByRefType(), typeof(TValue));
        }

        /// <summary>
        /// Generates a weakly-typed open-instance delegate to set the value of the field in a given target
        /// </summary>
        public static MemberSetter<object, object> DelegateForSet(this FieldInfo field)
        {
            return DelegateForSet<object, object>(field);
        }


        /// <summary>
        /// Generates a assembly called 'name' that's useful for debugging purposes and inspecting the resulting C# code in ILSpy
        /// If 'field' is not null, it generates a setter and getter for that field
        /// If 'property' is not null, it generates a setter and getter for that property
        /// If 'method' is not null, it generates a call for that method
        /// if 'targetType' and 'ctorParamTypes' are not null, it generates a constructor for the target type that takes the specified arguments
        /// </summary>
        public static void GenDebugAssembly(string name, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
        {
            GenDebugAssembly<object>(name, field, property, method, targetType, ctorParamTypes);
        }

        /// <summary>
        /// Generates a assembly called 'name' that's useful for debugging purposes and inspecting the resulting C# code in ILSpy
        /// If 'field' is not null, it generates a setter and getter for that field
        /// If 'property' is not null, it generates a setter and getter for that property
        /// If 'method' is not null, it generates a call for that method
        /// if 'targetType' and 'ctorParamTypes' are not null, it generates a constructor for the target type that takes the specified arguments
        /// </summary>
        public static void GenDebugAssembly<TTarget>(string name, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
        {
            var asmName = new AssemblyName("Asm");
            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
            var modBuilder = asmBuilder.DefineDynamicModule("Mod", name);
            var typeBuilder = modBuilder.DefineType("Test", TypeAttributes.Public);

            var weakTyping = typeof(TTarget) == typeof(object);

            Func<string, Type, Type[], ILGenerator> buildMethod = (methodName, returnType, parameterTypes) =>
            {
                var methodBuilder = typeBuilder.DefineMethod(methodName,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
                    CallingConventions.Standard,
                    returnType, parameterTypes);
                return methodBuilder.GetILGenerator();
            };

            if (field != null)
            {
                var fieldType = weakTyping ? typeof(object) : field.FieldType;
                emit.il = buildMethod("FieldSetter", typeof(void), new Type[] { typeof(TTarget).MakeByRefType(), fieldType });
                GenFieldSetter<TTarget>(field);
                emit.il = buildMethod("FieldGetter", fieldType, new Type[] { typeof(TTarget) });
                GenFieldGetter<TTarget>(field);
            }

            typeBuilder.CreateType();
            asmBuilder.Save(name);
        }

        static int GetKey<T, R>(MemberInfo member, string dynMethodName)
        {
            return member.GetHashCode() ^ dynMethodName.GetHashCode() ^ typeof(T).GetHashCode() ^ typeof(R).GetHashCode();
        }

        static TDelegate GenDelegateForMember<TDelegate, TMember>(TMember member, int key, string dynMethodName,
            Action<TMember> generator, Type returnType, params Type[] paramTypes)
            where TMember : MemberInfo
            where TDelegate : class
        {
            var dynMethod = new DynamicMethod(dynMethodName, returnType, paramTypes, true);

            emit.il = dynMethod.GetILGenerator();
            generator(member);

            var result = dynMethod.CreateDelegate(typeof(TDelegate));
            cache[key] = result;
            return (TDelegate)(object)result;
        }

        static void GenFieldGetter<TTarget>(FieldInfo field)
        {
            GenMemberGetter<TTarget>(field, field.FieldType, field.IsStatic,
                (e, f) =>
                {
                    if (field.IsLiteral)
                    {
                        if (field.FieldType == typeof(bool))
                            e.ldc_i4_1();
                        else if(field.FieldType == typeof(int))
                            e.ldc_i4((int) field.GetRawConstantValue());
                        else if (field.FieldType == typeof(float))
                            e.ldc_r4((float) field.GetRawConstantValue());
                        else if (field.FieldType == typeof(double))
                            e.ldc_r8((double)field.GetRawConstantValue());
                        else if (field.FieldType == typeof(string))
                            e.ldstr((string) field.GetRawConstantValue());
                        else
                            throw new NotSupportedException(string.Format("Creating a FieldGetter for type: {0} is unsupported.", field.FieldType.Name));
                    }
                    else
                        e.lodfld((FieldInfo) f);
                });
        }

        static void GenMemberGetter<TTarget>(MemberInfo member, Type memberType, bool isStatic, Action<ILEmitter, MemberInfo> get)
        {
            if (typeof(TTarget) == typeof(object)) // weakly-typed?
            {
                // if we're static immediately load member and return value
                // otherwise load and cast target, get the member value and box it if neccessary:
                // return ((DeclaringType)target).member;
                if (!isStatic)
                    emit.ldarg0()
                        .unboxorcast(member.DeclaringType);
                emit.perform(get, member)
                    .ifvaluetype_box(memberType);
            }
            else // we're strongly-typed, don't need any casting or boxing
            {
                // if we're static return member value immediately
                // otherwise load target and get member value immeidately
                // return target.member;
                if (!isStatic)
                    emit.ifclass_ldarg_else_ldarga(0, typeof(TTarget));
                emit.perform(get, member);
            }

            emit.ret();
        }

        static void GenFieldSetter<TTarget>(FieldInfo field)
        {
            GenMemberSetter<TTarget>(field, field.FieldType, field.IsStatic,
                (e, f) => e.setfld((FieldInfo)f)
            );
        }

        static void GenMemberSetter<TTarget>(MemberInfo member, Type memberType, bool isStatic, Action<ILEmitter, MemberInfo> set)
        {
            var targetType = typeof(TTarget);
            var stronglyTyped = targetType != typeof(object);

            // if we're static set member immediately
            if (isStatic)
            {
                emit.ldarg1();
                if (!stronglyTyped)
                    emit.unbox_any(memberType);
                emit.perform(set, member)
                    .ret();
                return;
            }

            if (stronglyTyped)
            {
                // push target and value argument, set member immediately
                // target.member = value;
                emit.ldarg0()
                    .ifclass_ldind_ref(targetType)
                    .ldarg1()
                    .perform(set, member)
                    .ret();
                return;
            }

            // we're weakly-typed
            targetType = member.DeclaringType;
            if (!targetType.IsValueType) // are we a reference-type?
            {
                // load and cast target, load and cast value and set
                // ((TargetType)target).member = (MemberType)value;
                emit.ldarg0()
                    .ldind_ref()
                    .cast(targetType)
                    .ldarg1()
                    .unbox_any(memberType)
                    .perform(set, member)
                    .ret();
                return;
            }

            // we're a value-type
            // handle boxing/unboxing for the user so he doesn't have to do it himself
            // here's what we're basically generating (remember, we're weakly typed, so
            // the target argument is of type object here):
            // TargetType tmp = (TargetType)target; // unbox
            // tmp.member = (MemberField)value;		// set member value
            // target = tmp;						// box back

            emit.declocal(targetType);
            emit.ldarg0()
                .ldind_ref()
                .unbox_any(targetType)
                .stloc0()
                .ldloca(0)
                .ldarg1()
                .unbox_any(memberType)
                .perform(set, member)
                .ldarg0()
                .ldloc0()
                .box(targetType)
                .stind_ref()
                .ret();
        }

        private class ILEmitter
        {
            public ILGenerator il;

            public ILEmitter ret()                                 { il.Emit(OpCodes.Ret); return this; }
            public ILEmitter cast(Type type)                       { il.Emit(OpCodes.Castclass, type); return this; }
            public ILEmitter box(Type type)                        { il.Emit(OpCodes.Box, type); return this; }
            public ILEmitter unbox_any(Type type)                  { il.Emit(OpCodes.Unbox_Any, type); return this; }
            public ILEmitter unbox(Type type)                      { il.Emit(OpCodes.Unbox, type); return this; }
            public ILEmitter call(MethodInfo method)               { il.Emit(OpCodes.Call, method); return this; }
            public ILEmitter callvirt(MethodInfo method)           { il.Emit(OpCodes.Callvirt, method); return this; }
            public ILEmitter ldnull()                              { il.Emit(OpCodes.Ldnull); return this; }
            public ILEmitter bne_un(Label target)                  { il.Emit(OpCodes.Bne_Un, target); return this; }
            public ILEmitter beq(Label target)                     { il.Emit(OpCodes.Beq, target); return this; }
            public ILEmitter ldc_i4_0()                            { il.Emit(OpCodes.Ldc_I4_0); return this; }
            public ILEmitter ldc_i4_1()                            { il.Emit(OpCodes.Ldc_I4_1); return this; }
            public ILEmitter ldc_i4(int c)                         { il.Emit(OpCodes.Ldc_I4, c); return this; }
            public ILEmitter ldc_r4(float c)                       { il.Emit(OpCodes.Ldc_R4, c); return this; }
            public ILEmitter ldc_r8(double c)                      { il.Emit(OpCodes.Ldc_R8, c); return this; }
            public ILEmitter ldarg0()                              { il.Emit(OpCodes.Ldarg_0); return this; }
            public ILEmitter ldarg1()                              { il.Emit(OpCodes.Ldarg_1); return this; }
            public ILEmitter ldarg2()                              { il.Emit(OpCodes.Ldarg_2); return this; }
            public ILEmitter ldarga(int idx)                       { il.Emit(OpCodes.Ldarga, idx); return this; }
            public ILEmitter ldarga_s(int idx)                     { il.Emit(OpCodes.Ldarga_S, idx); return this; }
            public ILEmitter ldarg(int idx)                        { il.Emit(OpCodes.Ldarg, idx); return this; }
            public ILEmitter ldarg_s(int idx)                      { il.Emit(OpCodes.Ldarg_S, idx); return this; }
            public ILEmitter ldstr(string str)                     { il.Emit(OpCodes.Ldstr, str); return this; }
            public ILEmitter ifclass_ldind_ref(Type type)		   { if (!type.IsValueType) il.Emit(OpCodes.Ldind_Ref); return this; }
            public ILEmitter ldloc0()                              { il.Emit(OpCodes.Ldloc_0); return this; }
            public ILEmitter ldloc1()                              { il.Emit(OpCodes.Ldloc_1); return this; }
            public ILEmitter ldloc2()                              { il.Emit(OpCodes.Ldloc_2); return this; }
            public ILEmitter ldloca_s(int idx)                     { il.Emit(OpCodes.Ldloca_S, idx); return this; }
            public ILEmitter ldloca_s(LocalBuilder local)          { il.Emit(OpCodes.Ldloca_S, local); return this; }
            public ILEmitter ldloc_s(int idx)                      { il.Emit(OpCodes.Ldloc_S, idx); return this; }
            public ILEmitter ldloc_s(LocalBuilder local)           { il.Emit(OpCodes.Ldloc_S, local); return this; }
            public ILEmitter ldloca(int idx)                       { il.Emit(OpCodes.Ldloca, idx); return this; }
            public ILEmitter ldloca(LocalBuilder local)            { il.Emit(OpCodes.Ldloca, local); return this; }
            public ILEmitter ldloc(int idx)                        { il.Emit(OpCodes.Ldloc, idx); return this; }
            public ILEmitter ldloc(LocalBuilder local)             { il.Emit(OpCodes.Ldloc, local); return this; }
            public ILEmitter initobj(Type type)                    { il.Emit(OpCodes.Initobj, type); return this; }
            public ILEmitter newobj(ConstructorInfo ctor)          { il.Emit(OpCodes.Newobj, ctor); return this; }
            public ILEmitter Throw()                               { il.Emit(OpCodes.Throw); return this; }
            public ILEmitter throw_new(Type type)                  { var exp = type.GetConstructor(Type.EmptyTypes); newobj(exp).Throw(); return this; }
            public ILEmitter stelem_ref()                          { il.Emit(OpCodes.Stelem_Ref); return this; }
            public ILEmitter ldelem_ref()                          { il.Emit(OpCodes.Ldelem_Ref); return this; }
            public ILEmitter ldlen()                               { il.Emit(OpCodes.Ldlen); return this; }
            public ILEmitter stloc(int idx)                        { il.Emit(OpCodes.Stloc, idx); return this; }
            public ILEmitter stloc_s(int idx)                      { il.Emit(OpCodes.Stloc_S, idx); return this; }
            public ILEmitter stloc(LocalBuilder local)             { il.Emit(OpCodes.Stloc, local); return this; }
            public ILEmitter stloc_s(LocalBuilder local)           { il.Emit(OpCodes.Stloc_S, local); return this; }
            public ILEmitter stloc0()                              { il.Emit(OpCodes.Stloc_0); return this; }
            public ILEmitter stloc1()                              { il.Emit(OpCodes.Stloc_1); return this; }
            public ILEmitter mark(Label label)                     { il.MarkLabel(label); return this; }
            public ILEmitter ldfld(FieldInfo field)                { il.Emit(OpCodes.Ldfld, field); return this; }
            public ILEmitter ldsfld(FieldInfo field)               { il.Emit(OpCodes.Ldsfld, field); return this; }
            public ILEmitter lodfld(FieldInfo field)               { if (field.IsStatic) ldsfld(field); else ldfld(field); return this; }
            public ILEmitter ifvaluetype_box(Type type)            { if (type.IsValueType) il.Emit(OpCodes.Box, type); return this; }
            public ILEmitter stfld(FieldInfo field)                { il.Emit(OpCodes.Stfld, field); return this; }
            public ILEmitter stsfld(FieldInfo field)               { il.Emit(OpCodes.Stsfld, field); return this; }
            public ILEmitter setfld(FieldInfo field)               { if (field.IsStatic) stsfld(field); else stfld(field); return this; }
            public ILEmitter unboxorcast(Type type)                { if (type.IsValueType) unbox(type); else cast(type); return this; }
            public ILEmitter callorvirt(MethodInfo method)         { if (method.IsVirtual) il.Emit(OpCodes.Callvirt, method); else il.Emit(OpCodes.Call, method); return this; }
            public ILEmitter stind_ref()                           { il.Emit(OpCodes.Stind_Ref); return this; }
            public ILEmitter ldind_ref()                           { il.Emit(OpCodes.Ldind_Ref); return this; }
            public LocalBuilder declocal(Type type)                { return il.DeclareLocal(type); }
            public Label deflabel()                                { return il.DefineLabel(); }
            public ILEmitter ifclass_ldarg_else_ldarga(int idx, Type type) { if (type.IsValueType) emit.ldarga(idx); else emit.ldarg(idx); return this; }
            public ILEmitter ifclass_ldloc_else_ldloca(int idx, Type type) { if (type.IsValueType) emit.ldloca(idx); else emit.ldloc(idx); return this; }
            public ILEmitter perform(Action<ILEmitter, MemberInfo> action, MemberInfo member) { action(this, member); return this; }
            public ILEmitter ifbyref_ldloca_else_ldloc(LocalBuilder local, Type type) { if (type.IsByRef) ldloca(local); else ldloc(local); return this; }
        }
    }
#endif

public static class ListExtensions
{
    #if EMIT
		public static class InternalAccessor<T> {
			public static MemberGetter<List<T>, T[]> ArrayGetter;
			public static MemberSetter<List<T>, int> CountSetter;
			
			static InternalAccessor ()
			{
				var type = typeof(List<T>);
				ArrayGetter = FastReflection.DelegateForGet<List<T>, T[]>(type.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
				CountSetter = FastReflection.DelegateForSet<List<T>, int>(type.GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance));
			}
		}
       
		
		public static T[] GetInternalArray<T> (this List<T> list)
		{
            return InternalAccessor<T>.ArrayGetter(list);
        }
		
		public static void SetInternalCount<T> (this List<T> list, int count)
		{
			if (list.Capacity < count)
                list.Capacity = count;
            InternalAccessor<T>.CountSetter(ref list, count);
        }
		
    #endif
		

        static public List<T> GetRangePool<T> (this List<T> source, int index, int count)
        {
            List<T> list = ListPool<T>.Get();
			// Method 0: Usual step to crop list (but GC is a sidekick here)
            //The only known thing that Produces GC, 
            //however, it currently the fastest thing to process
            //So I leave as it is,
            //return source.GetRange(index, count);
			//Method 1: Not working (Still Produces GC)
            /*
            T[] a = new T[count];
            source.CopyTo(index, a, 0, count);
            list.AddRange(a);
            */
            //Method 2: Too Slow
            /*
            for (int i = 0; i < count; i++)
            {
                list.Add(source[i + index]);
            }
             */
 #if !EMIT
            //Method 3
            //CHAMPAGNE! It's works ;-D
            list.AddRange(source);
			if(index > 0)
				list.RemoveRange(0, index);
			if(list.Count > count)
				list.RemoveRange(count, list.Count - count);
			return list;
#else
			//Method 4 : Using reflection to get the internal array directly
			// This trims down composition overhead by 90% :)
			// All hail to vexe: https://github.com/vexe/Fast.Reflection

            var array = source.GetInternalArray();
            list.SetInternalCount(count);
            var dest = list.GetInternalArray();
            System.Array.Copy(array, index, dest, 0, count);
            return list;
#endif
        }
		
		
}

}