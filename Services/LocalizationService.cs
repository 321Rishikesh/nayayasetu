using NyayaSetu.Interfaces;

namespace NyayaSetu.Services
{
    public class LocalizationService : ILocalizationService
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["AppName"] = "NyayaSetu",
                ["Tagline"] = "Smart Ministry of Law & Justice Management System",
                ["PrototypeNotice"] = "Academic Prototype Only — Not an Official Government of India Website",
                ["NavHome"] = "Home",
                ["NavAbout"] = "About",
                ["NavServices"] = "Services",
                ["NavLegalAid"] = "Legal Aid",
                ["NavTrack"] = "Track Application",
                ["NavGrievances"] = "Grievances",
                ["NavLogin"] = "Login",
                ["HeroTitle"] = "Transparent, Citizen-First Digital Legal Services",
                ["HeroSubtitle"] = "Submit legal service applications, track real-time progress, register grievances, and request free legal aid through one unified academic prototype.",
                ["TrackPlaceholder"] = "Enter Tracking ID (e.g., NYS-2026-1001) or search services...",
                ["SearchBtn"] = "Track / Search"
            },
            ["hi"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["AppName"] = "न्यायसेतु (NyayaSetu)",
                ["Tagline"] = "स्मार्ट विधि एवं न्याय प्रबंधन प्रणाली",
                ["PrototypeNotice"] = "केवल शैक्षणिक प्रोटोटाइप — भारत सरकार की आधिकारिक वेबसाइट नहीं है",
                ["NavHome"] = "मुख्य पृष्ठ",
                ["NavAbout"] = "परिचय",
                ["NavServices"] = "सेवाएं",
                ["NavLegalAid"] = "विधिक सहायता",
                ["NavTrack"] = "आवेदन ट्रैक करें",
                ["NavGrievances"] = "शिकायतें",
                ["NavLogin"] = "लॉगिन",
                ["HeroTitle"] = "पारदर्शी और नागरिक-केंद्रित डिजिटल विधिक सेवाएं",
                ["HeroSubtitle"] = "विधिक सेवा आवेदन जमा करें, रियल-टाइम स्थिति ट्रैक करें, शिकायत दर्ज करें और निःशुल्क विधिक सहायता के लिए आवेदन करें।",
                ["TrackPlaceholder"] = "ट्रैकिंग आईडी दर्ज करें (जैसे NYS-2026-1001)...",
                ["SearchBtn"] = "ट्रैक / खोजें"
            },
            ["pa"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["AppName"] = "ਨਿਆਇਸੇਤੂ (NyayaSetu)",
                ["Tagline"] = "ਸਮਾਰਟ ਕਾਨੂੰਨ ਅਤੇ ਨਿਆਂ ਪ੍ਰਬੰਧਨ ਪ੍ਰਣਾਲੀ",
                ["PrototypeNotice"] = "ਸਿਰਫ਼ ਅਕਾਦਮਿਕ ਪ੍ਰੋਟੋਟਾਈਪ — ਭਾਰਤ ਸਰਕਾਰ ਦੀ ਅਧਿਕਾਰਤ ਵੈੱਬਸਾਈਟ ਨਹੀਂ ਹੈ",
                ["NavHome"] = "ਮੁੱਖ ਪੰਨਾ",
                ["NavAbout"] = "ਬਾਰੇ",
                ["NavServices"] = "ਸੇਵਾਵਾਂ",
                ["NavLegalAid"] = "ਕਾਨੂੰਨੀ ਸਹਾਇਤਾ",
                ["NavTrack"] = "ਅਰਜ਼ੀ ਟਰੈਕ ਕਰੋ",
                ["NavGrievances"] = "ਸ਼ਿਕਾਇਤਾਂ",
                ["NavLogin"] = "ਲੌਗਇਨ",
                ["HeroTitle"] = "ਪਾਰਦਰਸ਼ੀ ਅਤੇ ਨਾਗਰਿਕ-ਕੇਂਦਰਿਤ ਡਿਜੀਟਲ ਕਾਨੂੰਨੀ ਸੇਵਾਵਾਂ",
                ["HeroSubtitle"] = "ਕਾਨੂੰਨੀ ਸੇਵਾ ਅਰਜ਼ੀਆਂ ਜਮ੍ਹਾਂ ਕਰੋ, ਰੀਅਲ-ਟਾਈਮ ਸਥਿਤੀ ਟਰੈਕ ਕਰੋ, ਸ਼ਿਕਾਇਤਾਂ ਦਰਜ ਕਰੋ ਅਤੇ ਮੁਫ਼ਤ ਕਾਨੂੰਨੀ ਸਹਾਇਤਾ ਦੀ ਬੇਨਤੀ ਕਰੋ।",
                ["TrackPlaceholder"] = "ਟਰੈਕਿੰਗ ਆਈਡੀ ਦਰਜ ਕਰੋ (ਜਿਵੇਂ NYS-2026-1001)...",
                ["SearchBtn"] = "ਟਰੈਕ / ਖੋਜੋ"
            }
        };

        public string NormalizeLanguage(string? lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) return "en";
            var lower = lang.Trim().ToLowerInvariant();
            return Translations.ContainsKey(lower) ? lower : "en";
        }

        public string Get(string key, string? lang = null)
        {
            var code = NormalizeLanguage(lang);
            if (Translations[code].TryGetValue(key, out var val))
                return val;
            return Translations["en"].TryGetValue(key, out var fallback) ? fallback : key;
        }
    }
}
