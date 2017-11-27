#if UNITY_EDITOR


//A Class contain font header data, discarded on build
namespace TexDrawLib
{
    [System.Serializable]
    public class TexFontHeader
    {
        public string caption;
        public int contentLength;
        public bool opened;

        public TexFontHeader()
        {
        }

        public TexFontHeader(string Caption, int ContentLength)
        {
            caption = Caption;
            opened = true;
            contentLength = ContentLength;
        }
    }

    [System.Serializable]
    public class TEXConfigurationMember
    {
        public string name;
        public string desc;
        public float value;
        public float defaultValue;
        public float min;
        public float max;

        public TEXConfigurationMember()
        {
        }

        public TEXConfigurationMember(string Name, string Desc, float Value, float Min, float Max)
        {
            name = Name;
            desc = Desc;
            value = Value;
            defaultValue = Value;
            min = Min;
            max = Max;
        }
    }
}
#endif
