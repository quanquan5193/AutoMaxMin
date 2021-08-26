using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AutoClick.Model
{
    public class Common
    {
        public const string IIS_IUSRS = "IIS_IUSRS";

        public static void CreateDirectoryAndGrantFullControlPermission(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    DirectoryInfo dInfo = Directory.CreateDirectory(folderPath);
                    GrantFullControlPermissionForDirectory(IIS_IUSRS, folderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void GrantFullControlPermissionForDirectory(string identity, string folderPath)
        {
            //IIS_IUSRS
            try
            {
                bool modified;
                FileSystemRights fileSystemRights = FileSystemRights.FullControl;
                InheritanceFlags inheritanceFlags = InheritanceFlags.None;
                FileSystemAccessRule accessRule = new FileSystemAccessRule(identity, fileSystemRights, inheritanceFlags, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
                DirectoryInfo dInfo = new DirectoryInfo(folderPath);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.ModifyAccessRule(AccessControlModification.Set, accessRule, out modified);
                InheritanceFlags iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                FileSystemAccessRule inheritancedAccessRule = new FileSystemAccessRule(identity, fileSystemRights, iFlags, PropagationFlags.InheritOnly, AccessControlType.Allow);
                dSecurity.ModifyAccessRule(AccessControlModification.Add, inheritancedAccessRule, out modified);
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        
    }

    public static class StaticEnum
    {
        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStringValue(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }
    }

    public enum ClickType
    {
        click = 0,
        rightClick = 1,
        doubleClick = 2,
    }

    public enum ButtonType
    {
        Tai,
        Xiu,
        Cuoc,
        Other
    }

    public enum BetType
    {
        Tai,
        Xiu
    }

    public enum ResultStatus
    {
        Win,
        Lose,
        Draw,
        Running
    }

    /// <summary>
    /// This attribute is used to represent a string value
    /// for a value in an enum.
    /// </summary>
    public class StringValueAttribute : Attribute
    {

        #region Properties

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }

        #endregion

    }

    
}
