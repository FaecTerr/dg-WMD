using System;
using System.Collections.Generic;
using DuckGame;

namespace DuckGame.WMD
{
    public class NetMessageAdder
    {
        public static void UpdateNetmessageTypes()
        {
            IEnumerable<Type> subclasses = Editor.GetSubclasses(typeof(NetMessage));
            Network.typeToMessageID.Clear();
            ushort key = 1;
            foreach (Type type in subclasses)
            {
                bool flag = type.GetCustomAttributes(typeof(FixedNetworkID), false).Length != 0;
                if (flag)
                {
                    FixedNetworkID customAttribute = (FixedNetworkID)type.GetCustomAttributes(typeof(FixedNetworkID), false)[0];
                    bool flag2 = customAttribute != null;
                    if (flag2)
                    {
                        Network.typeToMessageID.Add(type, customAttribute.FixedID);
                    }
                }
            }
            foreach (Type type2 in subclasses)
            {
                bool flag3 = !Network.typeToMessageID.ContainsValue(type2);
                if (flag3)
                {
                    while (Network.typeToMessageID.ContainsKey(key))
                    {
                        key += 1;
                    }
                    Network.typeToMessageID.Add(type2, key);
                    key += 1;
                }
            }
        }
    }
}
