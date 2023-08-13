namespace HealthCenter
{
    public class EmployeeNumber
    {
        public int value { get; set; }

        public EmployeeNumber(int value)
        {
            this.value = value;
        }

        public EmployeeNumber(string value) : this(int.Parse(value.Replace(" ", "")))
        {
        }
    }
}
