using NyayaSetu.Interfaces;

namespace NyayaSetu.Services
{
    /// <summary>
    /// Verified-source Legal Information Assistant for the NyayaSetu academic prototype.
    /// Explains legal terminology and service workflows in plain language, cites official Indian
    /// legal portals, and never fabricates case-specific legal advice.
    /// </summary>
    public class AiAssistantService : IAiAssistantService
    {
        private const string StandardDisclaimer =
            "Disclaimer: NyayaSetu is an academic prototype. This assistant provides general legal information from public official sources only and does NOT replace a qualified advocate or constitute formal legal advice.";

        public AssistantReply Ask(string question, string language = "en")
        {
            var q = (question ?? string.Empty).Trim().ToLowerInvariant();
            var sources = new List<AssistantResourceLink>();

            if (string.IsNullOrWhiteSpace(q))
            {
                return new AssistantReply(
                    "Please enter a question about legal terms (such as affidavit, Lok Adalat, FIR, RTI), free legal aid eligibility, or how to submit and track an application on NyayaSetu.",
                    new List<AssistantResourceLink>
                    {
                        new("Ministry of Law and Justice", "https://lawmin.gov.in/"),
                        new("National Legal Services Authority (NALSA)", "https://nalsa.gov.in/")
                    },
                    StandardDisclaimer);
            }

            if (q.Contains("legal aid") || q.Contains("nalsa") || q.Contains("free lawyer") || q.Contains("eligibility") || q.Contains("tele-law"))
            {
                sources.Add(new("NALSA Legal Aid Portal", "https://nalsa.gov.in/legal-aid/"));
                sources.Add(new("Tele-Law Portal (Department of Justice)", "https://www.tele-law.in/"));
                sources.Add(new("Punjab State Legal Services Authority", "https://punjab.nalsa.gov.in/"));

                return new AssistantReply(
                    "Under Section 12 of the Legal Services Authorities Act, 1987, free legal aid is available to eligible citizens including women and children, members of SC/ST communities, industrial workmen, persons with disabilities, victims of mass disaster/violence, persons in custody, and citizens whose annual income falls below the prescribed state threshold. In NyayaSetu, you can apply from the Legal Aid section and upload supporting proof for officer review.",
                    sources,
                    StandardDisclaimer);
            }

            if (q.Contains("lok adalat") || q.Contains("mediation") || q.Contains("adr") || q.Contains("dispute"))
            {
                sources.Add(new("NALSA Lok Adalats", "https://nalsa.gov.in/lok-adalats/"));
                sources.Add(new("Department of Justice", "https://doj.gov.in/"));

                return new AssistantReply(
                    "A Lok Adalat (People's Court) is an Alternative Dispute Resolution (ADR) forum organized under the Legal Services Authorities Act, 1987, where pending or pre-litigation disputes are settled amicably by compromise. There is no court fee, and a Lok Adalat award is deemed a civil court decree.",
                    sources,
                    StandardDisclaimer);
            }

            if (q.Contains("ecourt") || q.Contains("case status") || q.Contains("cnr") || q.Contains("judgment"))
            {
                sources.Add(new("eCourts Services Portal", "https://services.ecourts.gov.in/"));
                sources.Add(new("eCourts Judgments Search", "https://judgments.ecourts.gov.in/"));

                return new AssistantReply(
                    "To check real court case status or daily orders in India, use the official eCourts Services portal with your 16-digit CNR (Case Number Record) number, party name, or advocate name. For NyayaSetu prototype applications, enter your tracking ID (e.g., NYS-2026-1001) on the Track Application page.",
                    sources,
                    StandardDisclaimer);
            }

            if (q.Contains("affidavit") || q.Contains("notary") || q.Contains("attest"))
            {
                sources.Add(new("Ministry of Law and Justice", "https://lawmin.gov.in/"));

                return new AssistantReply(
                    "An affidavit is a written statement of facts voluntarily confirmed by oath or affirmation before an authorized Notary Public or Oath Commissioner. In NyayaSetu, you can submit a Notary Certificate or Legal Document Attestation request under the Department of Legal Affairs.",
                    sources,
                    StandardDisclaimer);
            }

            if (q.Contains("track") || q.Contains("status") || q.Contains("complaint") || q.Contains("grievance"))
            {
                sources.Add(new("Department of Justice", "https://doj.gov.in/"));

                return new AssistantReply(
                    "To track an application on NyayaSetu, go to 'Track Application' and enter your unique tracking ID (format: NYS-YYYY-XXXX). You can view the current stage, assigned department, officer remarks, and full timeline. Overdue grievances are automatically flagged and escalated according to priority SLA rules.",
                    sources,
                    StandardDisclaimer);
            }

            sources.Add(new("Ministry of Law and Justice", "https://lawmin.gov.in/"));
            sources.Add(new("Department of Justice", "https://doj.gov.in/"));
            sources.Add(new("NALSA Official Portal", "https://nalsa.gov.in/"));

            return new AssistantReply(
                "I can explain common Indian legal service terms (Legal Aid / NALSA, Lok Adalat & Mediation, Affidavits & Notary, eCourts CNR tracking) and guide you through NyayaSetu's application, grievance, and consultation modules. For case-specific legal opinions, please consult a qualified advocate or apply for Free Legal Aid.",
                sources,
                StandardDisclaimer);
        }
    }
}
