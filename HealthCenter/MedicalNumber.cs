namespace HealthCenter
{
    public class MedicalNumber
    {
        public int value { get; set; }

        public MedicalNumber(int value)
        {
            this.value = value;
        }

        public MedicalNumber(string value) : this(int.Parse(value.Replace(" ", "")))
        {
        }
    }
}
