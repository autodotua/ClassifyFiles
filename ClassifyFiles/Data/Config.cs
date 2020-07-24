namespace ClassifyFiles.Data
{
    public class Config : DbModelBase
    {
        public Config()
        {

        }
        public Config(string key, string value)
        {
            Key = key;
            Value = value;
        }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
