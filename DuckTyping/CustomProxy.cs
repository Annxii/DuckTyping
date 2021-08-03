using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace DuckTyping
{
    class CustomProxy<T>
    {
        private static MethodInfo writeLineInfo = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
        private static MethodInfo toStringInfo = typeof(object).GetMethod("ToString");

        private static AssemblyBuilder asm;
        private static ModuleBuilder module;
        static CustomProxy()
        {
            asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicProxies"), AssemblyBuilderAccess.Run);
            module = asm.DefineDynamicModule("Anx.DynamicProxies", emitSymbolInfo: true);
        }

        private static string GetTypeName() => $"{typeof(T).Name}ProxyImpl";

        public static T Wrap(T instance)
        {
            if (!typeof(T).IsInterface)
                throw new NotSupportedException("Only interfaces are supported");

            var name = GetTypeName();
            var type = module.GetType(name) ?? CreateDecorator(name);

            return (T)Activator.CreateInstance(type, instance);
        }

        private static Type CreateDecorator(string name)
        {
            var originalType = typeof(T);
            var type = module.DefineType(name, TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Class | TypeAttributes.AutoLayout);
            type.AddInterfaceImplementation(originalType);

            FieldInfo instanceField;
            DefineConstructor(type, originalType, out instanceField);

            foreach (var m in originalType.GetMethods())
            {
                DefineMethod(type, m, originalType, instanceField);
            }

            return type.CreateType();
        }

        private static void DefineConstructor(TypeBuilder type, Type originalType, out FieldInfo instanceField)
        {
            instanceField = type.DefineField("instance", typeof(T), FieldAttributes.Private);

            var ctor = type.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                CallingConventions.Standard, new[] { originalType });
            ctor.SetImplementationFlags(MethodImplAttributes.Managed);
            ctor.DefineParameter(0, ParameterAttributes.None, "x");
            var gen = ctor.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, instanceField);

            gen.Emit(OpCodes.Ret);
        }

        private static void DefineMethod(TypeBuilder type, MethodInfo m, Type originalType, FieldInfo instanceField)
        {
            var isVoid = m.ReturnType == null || m.ReturnType == typeof(void);
            var parameters = m.GetParameters();
            var method = type.DefineMethod(m.Name, MethodAttributes.Public | MethodAttributes.Virtual, m.ReturnType, parameters.Select(x => x.ParameterType).ToArray());
            type.DefineMethodOverride(method, m);
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = method.DefineParameter(i, ParameterAttributes.None, parameters[i].Name);
            }

            var gen = method.GetILGenerator();

            gen.DeclareLocal(typeof(DateTime)); // idx 0
            gen.DeclareLocal(typeof(DateTime)); // idx 1
            gen.DeclareLocal(typeof(TimeSpan)); // idx 2
            if (!isVoid)
                gen.DeclareLocal(m.ReturnType); // idx 3

            gen.Emit(OpCodes.Ldstr, $"Begin {m.Name}");
            gen.Emit(OpCodes.Call, writeLineInfo);

            gen.Emit(OpCodes.Call, typeof(DateTime).GetProperty("Now").GetMethod);
            gen.Emit(OpCodes.Stloc_0);

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, instanceField);
            for (int i = 0; i < parameters.Length; i++)
            {
                gen.Emit(OpCodes.Ldarg, i + 1);
            }

            gen.EmitCall(OpCodes.Callvirt, m, null);
            if (!isVoid)
                gen.Emit(OpCodes.Stloc_3);

            gen.Emit(OpCodes.Call, typeof(DateTime).GetProperty("Now").GetMethod);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldloca, 1);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, typeof(DateTime).GetMethod("Subtract", new[] { typeof(DateTime) }));
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldloca, 2);
            gen.Emit(OpCodes.Constrained, typeof(TimeSpan));
            gen.Emit(OpCodes.Callvirt, toStringInfo);
            gen.Emit(OpCodes.Call, writeLineInfo);

            if (!isVoid)
            {
                gen.Emit(OpCodes.Ldloc_3);
                if(m.ReturnType != typeof(string))
                    gen.Emit(OpCodes.Call, toStringInfo);

                gen.Emit(OpCodes.Call, writeLineInfo);
            }

            gen.Emit(OpCodes.Ldstr, $"End {m.Name}");
            gen.Emit(OpCodes.Call, writeLineInfo);

            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
        }
    }
}
