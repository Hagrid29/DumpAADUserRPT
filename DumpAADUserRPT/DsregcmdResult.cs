using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GetAadJoinInformation
{
    public class DsregcmdResult
    {
        public JoinType JoinType { get; set; }
        public Guid DeviceId { get; set; }
        public string IdpDomain { get; set; }
        public Guid TenantId { get; set; }
        public string JoinUserEmail { get; set; }

        public string TenantDisplayName { get; set; }

        public string MdmEnrollmentUrl { get; set; }
        public string MdmTermsOfUseUrl { get; set; }
        public string MdmComplianceUrl { get; set; }
        public string UserSettingSyncUrl { get; set; }
        public string UserInfoEmail { get; set; }
        public Guid UserInfoKeyId { get; set; }
        public string UserInfoKeyname { get; set; }
        public List<X509Certificate2> CertInfo { get; set; }
    }

}
