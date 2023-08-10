using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FactoryBot.Configurations
{
    public class MemberEqualityComparer : EqualityComparer<MemberInfo>
    {
        public override bool Equals(MemberInfo? @this, MemberInfo? that)
        {
            if (@this == that)
            {   
                return true;
            }

            if (@this == null || that == null)
            {
                return false;
            }

            // handles everything except for generics
            if (@this.MetadataToken != that.MetadataToken
                || !Equals(@this.Module, that.Module)
                || @this.DeclaringType != that.DeclaringType)
            {
                return false;
            }

            bool areEqual;
            switch (@this.MemberType)
            {
                // constructors and methods can be generic independent of their types,
                // so they are equal if they're generic arguments are equal
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    var thisMethod = @this as MethodBase;
                    var thatMethod = that as MethodBase;
                    areEqual = thisMethod != null && thatMethod != null && thisMethod.GetGenericArguments()
                        .SequenceEqual(thatMethod.GetGenericArguments(), this);
                    break;
                // properties, events, and fields cannot be generic independent of their types,
                // so if we've reached this point without bailing out we just return true.
                case MemberTypes.Property:
                case MemberTypes.Event:
                case MemberTypes.Field:
                    areEqual = true;
                    break;
                // the system guarantees reference equality for types, so if we've reached this point
                // without returning true the two are not equal
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    areEqual = false;
                    break;
                default:
                    throw new NotImplementedException(@this.MemberType.ToString());
            }

            return areEqual;
        }

        public override int GetHashCode(MemberInfo? memberInfo)
        {
            if (memberInfo == null)
            {
                return 0;
            }

            var hash = memberInfo.MetadataToken
                       ^ memberInfo.Module.GetHashCode()
                       ^ GetHashCode(memberInfo.DeclaringType);
            return hash;
        }
    }
}