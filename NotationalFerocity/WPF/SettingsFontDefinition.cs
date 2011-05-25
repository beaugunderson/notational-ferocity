using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace NotationalFerocity.WPF
{
    [Serializable]
    [XmlRoot(ElementName = "Font", IsNullable = false)]
    public class SettingsFontDefinition : INotifyPropertyChanged
    {
        private string _fontFamily;

        [XmlAttribute]
        public string FontFamily
        {
            get
            {
                return _fontFamily;
            }

            set
            {
                _fontFamily = value;

                OnPropertyChanged("FontFamily");
            }
        }

        [XmlAttribute]
        public string FontStyle { get; set; }

        [XmlAttribute]
        public string FontWeight { get; set; }

        [XmlAttribute]
        public string FontStretch { get; set; }

        [XmlAttribute]
        public double FontSize { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public FontDefinition ToFontDefinition()
        {
            var styleConverter = new FontStyleConverter();
            var weightConverter = new FontWeightConverter();
            var stretchConverter = new FontStretchConverter();

            var style = (FontStyle?)styleConverter.ConvertFromString(FontStyle) ?? FontStyles.Normal;
            var weight = (FontWeight?)weightConverter.ConvertFromString(FontWeight) ?? FontWeights.Normal;
            var stretch = (FontStretch?)stretchConverter.ConvertFromString(FontStretch) ?? FontStretches.Normal;
            
            return new FontDefinition
            {
                FontFamily = new FontFamily(FontFamily),
                FontStyle = style,
                FontWeight = weight,
                FontStretch = stretch,
                FontSize = FontSize
            };
        }

        public static SettingsFontDefinition FromFontDefinition(FontDefinition font)
        {
            return new SettingsFontDefinition
            {
                FontFamily = font.FontFamily.ToString(),
                FontStyle = font.FontStyle.ToString(),
                FontWeight = font.FontWeight.ToString(),
                FontStretch =  font.FontStretch.ToString(),
                FontSize = font.FontSize
            };
        }
    }
}