using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Replay.Functions.Core {
    public class FileAssociation {
        public string Extension { get; set; }
        public string ProgId { get; set; }
        public string FileTypeDescription { get; set; }
        public string ExecutableFilePath { get; set; }
        public string iconPath { get; set; }
    }

    public class FileAssociations {
        private const int SHCNE_ASSOCCHANGED = 0x8000000;

        private const int SHCNF_FLUSH = 0x1000;

        // needed so that Explorer windows get refreshed after the registry is updated
        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        public static void EnsureAssociationsSet() {
            var filePath = Process.GetCurrentProcess().MainModule.FileName;
            EnsureAssociationsSet(
                new FileAssociation {
                    Extension = ".ucs",
                    ProgId = "UCS_Editor_File",
                    FileTypeDescription = "UCS File",
                    ExecutableFilePath = filePath
                });
        }

        public static void EnsureAssociationsSet(params FileAssociation[] associations) {
            var madeChanges = false;
            foreach (var association in associations)
                madeChanges |= SetAssociation(
                    association.Extension,
                    association.ProgId,
                    association.FileTypeDescription,
                    association.ExecutableFilePath,
                    association.iconPath);

            if (madeChanges) SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool SetAssociation(string extension, string progId, string fileTypeDescription,
            string applicationFilePath, string iconPath) {
            var madeChanges = false;
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, progId);
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + progId, fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command",
                "\"" + applicationFilePath + "\" \"%1\"");
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\DefaultIcon",
                "\"" + iconPath + "\"");
            return madeChanges;
        }
        
        public static void RemoveAssociation(string extension, string progId) {
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\" + extension, false);
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\" + progId, false);
        }

        private static bool SetKeyDefaultValue(string keyPath, string value) {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath)) {
                if (key.GetValue(null) as string != value) {
                    key.SetValue(null, value);
                    return true;
                }
            }

            return false;
        }
    }
}