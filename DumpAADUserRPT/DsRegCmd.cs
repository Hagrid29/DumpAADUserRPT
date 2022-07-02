using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using static GetAadJoinInformation.InternalData;

namespace GetAadJoinInformation
{
    public class Dsregcmd
    {
        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void NetFreeAadJoinInformation(
            IntPtr pJoinInfo);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int NetGetAadJoinInformation(
                string pcszTenantId,
                out IntPtr ppJoinInfo);

        public static DsregcmdResult GetInfo()
        {
            DsregcmdResult result = new DsregcmdResult();
            string tenantId = null;
            int retValue = NetGetAadJoinInformation(tenantId, out IntPtr ptrJoinInfo);
            if (retValue == 0)
            {

                var joinInfo = Marshal.PtrToStructure<DSREG_JOIN_INFO>(ptrJoinInfo);

                result.JoinType = (JoinType)joinInfo.joinType;

                Guid.TryParse(joinInfo.DeviceId, out Guid did);
                result.DeviceId = did;

                Guid.TryParse(joinInfo.TenantId, out Guid tid);
                result.TenantId = tid;

                result.IdpDomain = joinInfo.IdpDomain;
                result.JoinUserEmail = joinInfo.JoinUserEmail;
                result.MdmComplianceUrl = joinInfo.MdmComplianceUrl;
                result.MdmEnrollmentUrl = joinInfo.MdmEnrollmentUrl;
                result.MdmTermsOfUseUrl = joinInfo.MdmTermsOfUseUrl;
                result.TenantDisplayName = joinInfo.TenantDisplayName;

                byte[] data = System.Convert.FromBase64String(joinInfo.UserSettingSyncUrl);
                result.UserSettingSyncUrl = System.Text.ASCIIEncoding.ASCII.GetString(data);

                // Userinfo
                try
                {
                    var ptrUserInfo = joinInfo.pUserInfo;
                    if (ptrUserInfo != IntPtr.Zero)
                    {
                        var userInfo = Marshal.PtrToStructure<DSREG_USER_INFO>(ptrUserInfo);
                        result.UserInfoEmail = userInfo.UserEmail;
                        result.UserInfoKeyname = userInfo.UserKeyName;

                        Guid.TryParse(userInfo.UserKeyId, out Guid uid);
                        result.UserInfoKeyId = uid;
                    }
                }
                catch
                {

                }

                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

                store.Open(OpenFlags.ReadOnly);

                List<X509Certificate2> cresult = new List<X509Certificate2>();

                foreach (X509Certificate2 certificate in store.Certificates)
                {
                    if (certificate.Subject.Equals($"CN={result.DeviceId}"))
                    {
                        cresult.Add(certificate);
                    }
                }
                result.CertInfo = cresult;

                return result;
            }
            try { 
                Marshal.Release(ptrJoinInfo);
            } catch
            {

            }

            return new DsregcmdResult() { JoinType = JoinType.DSREG_NO_JOIN };
        }
    }
}
