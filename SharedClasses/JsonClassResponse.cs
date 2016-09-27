namespace SharedClasses {
    public class JsonClassResponse<T> {
        public int RID { get; set; }
        public int RequestCode { get; set; }
        public T Response { get; set; }
    }
}
