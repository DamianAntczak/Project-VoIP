namespace SharedClasses {
    public class JsonClassResponse<T> {
        public int RID { get; set; }
        public string IP { get; set; }
        public int Code { get; set; }
        public T Response { get; set; }
    }
}
