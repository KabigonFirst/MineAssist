using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MineAssist.Framework {
    public static class ReflectiveObjects {
        public static IEnumerable<T> GetEachObjectsOfSubtype<T>(params object[] constructorArgs) where T : class {
            foreach (Type type in
                    Assembly.GetAssembly(typeof(T)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))) {
                yield return (T)Activator.CreateInstance(type, constructorArgs);
            }
        }
        public static Dictionary<string, T> GetEachObjectDictOfSubtype<T>(params object[] constructorArgs) where T : class {
            Dictionary<string, T> dic = new Dictionary<string, T>();
            IEnumerable<T> objs = GetEachObjectsOfSubtype<T>(constructorArgs);
            foreach (var obj in objs) {
                dic[obj.GetType().Name] = obj;
            }
            return dic;
        }
    }
}
