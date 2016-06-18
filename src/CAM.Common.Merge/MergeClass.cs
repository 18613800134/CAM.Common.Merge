using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace CAM.Common.Merge
{
    public static class MergeClass
    {
        public static Type Merge(this Type Self, Type SourceClass)
        {
            return MergeTwoClass(Self, SourceClass);
        }

        private static Type MergeTwoClass(Type sourceA, Type sourceB)
        {
            //AssemblyName aName = new AssemblyName("MergeObjectAssembly");
            //AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            //ModuleBuilder mb = ab.DefineDynamicModule(aName.Name);
            //TypeBuilder tb = mb.DefineType("MergeObjectClass", TypeAttributes.Public);

            AppDomain myDomain = Thread.GetDomain();
            AssemblyName myAsmName = new AssemblyName();
            myAsmName.Name = "MergeObjectAssembly";

            AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.Run);
            ModuleBuilder myModBuilder = myAsmBuilder.DefineDynamicModule(myAsmName.Name);
            TypeBuilder myTypeBuilder = myModBuilder.DefineType("MergeObjectClass");

            if (sourceA != null)
            {
                CreateSameProperty(myTypeBuilder, sourceA, false);
            }
            if (sourceB != null)
            {
                CreateSameProperty(myTypeBuilder, sourceB);
            }
            Type mergeType = myTypeBuilder.CreateType();

            return mergeType;
        }

        private static void CreateSameProperty(TypeBuilder myTypeBuilder, Type source, bool withClassName = true)
        {
            MethodAttributes getSetAttr = MethodAttributes.Public |
                                          MethodAttributes.SpecialName |
                                          MethodAttributes.HideBySig;
            string className = withClassName ? string.Format("{0}_", source.Name) : "";
            foreach (PropertyInfo pi in source.GetProperties())
            {
                string propertyName = string.Format("{0}{1}", className, pi.Name);
                string fieldName = string.Format("m_{0}", propertyName);
                string getMethodName = string.Format("get_{0}", propertyName);
                string setMethodName = string.Format("set_{0}", propertyName);
                Type proType = pi.PropertyType;

                FieldBuilder fbProperty = myTypeBuilder.DefineField(fieldName, proType, FieldAttributes.Private);

                PropertyBuilder pbProperty = myTypeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, proType, Type.EmptyTypes);

                MethodBuilder myProGetAccessor = myTypeBuilder.DefineMethod(getMethodName, getSetAttr, proType, Type.EmptyTypes);
                ILGenerator proGetIL = myProGetAccessor.GetILGenerator();
                proGetIL.Emit(OpCodes.Ldarg_0);
                proGetIL.Emit(OpCodes.Ldfld, fbProperty);
                proGetIL.Emit(OpCodes.Ret);

                MethodBuilder myProSetAccessor = myTypeBuilder.DefineMethod(setMethodName, getSetAttr, null, new Type[] { proType });
                ILGenerator proSetIL = myProSetAccessor.GetILGenerator();
                proSetIL.Emit(OpCodes.Ldarg_0);
                proSetIL.Emit(OpCodes.Ldarg_1);
                proSetIL.Emit(OpCodes.Stfld, fbProperty);
                proSetIL.Emit(OpCodes.Ret);

                pbProperty.SetGetMethod(myProGetAccessor);
                pbProperty.SetSetMethod(myProSetAccessor);
            }
        }
    }

}
