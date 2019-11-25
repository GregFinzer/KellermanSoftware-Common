namespace KellermanSoftware.Common
{
    public static class WebUtil
    {
        public static string UrlCombineSafe(string baseUrl, string url)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return url;

            string result = string.Format("{0}/{1}", baseUrl.TrimEnd('/'), url.TrimStart('/'));

            if (!result.ToLower().StartsWith("http"))
            {
                result = "http://" + result;
            }

            return result;
        }
    }
}
