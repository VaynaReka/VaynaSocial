using System;
using UnityEngine.Networking;

namespace VaynaSocial
{
	// Token: 0x02000004 RID: 4
	public class AcceptAnyCertificateHandler : CertificateHandler
	{
		// Token: 0x0600001A RID: 26 RVA: 0x00002ED9 File Offset: 0x000010D9
		protected override bool ValidateCertificate(byte[] certificateData)
		{
			return true;
		}
	}
}
