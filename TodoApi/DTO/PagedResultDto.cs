namespace TodoApi.DTO
{
    /// <summary>
    /// �������������� ��������� ��� ������� API.
    /// �������� ���������� ��������� � �������� ������� ��������.
    /// </summary>
    /// <typeparam name="T">��� ��������� � ���������.</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// ����� ���������� ��������� ��� ����� ���������.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// ����� ������� �������� (������� � 1).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// ������ �������� (���������� ��������� �� ��������).
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// ��������� ��������� ������� ��������.
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
    }
}