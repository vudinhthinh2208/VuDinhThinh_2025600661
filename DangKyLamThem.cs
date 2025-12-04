using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DangKyLamThem
{
    public partial class Form1 : Form
    {
        private DataUtils dataUtils = new DataUtils();
        private List<NhanVien> nhanViens = new List<NhanVien>();
        public Form1()
        {
            InitializeComponent();
        }

        public void LoadData()
        {
            nhanViens.Clear();
            nhanViens = dataUtils.Show();
            dtgr_view.DataSource = nhanViens;
            dtgr_view.Columns["ngayLamViec"].HeaderText = "Ngày làm thêm";
            dtgr_view.Columns["maNV"].HeaderText = "Mã nhân viên";
            dtgr_view.Columns["loaiLamThem"].HeaderText = "Loại làm thêm";
            dtgr_view.Columns["soGio"].HeaderText = "Số giờ";
            dtgr_view.Columns["trangThai"].HeaderText = "Trạng thái";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btn_add_Click(object sender, EventArgs e)
        {
            string ngay = tb_date.Text.Trim();
            string ma = tb_code.Text.Trim();
            if (string.IsNullOrEmpty(ngay) || string.IsNullOrEmpty(ma))
            {
                MessageBox.Show("Trường NGÀY và MÃ NHÂN VIÊN là bắt buộc!",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Kiểm tra trùng
            if (dataUtils.CheckAdd(ngay, ma))
            {
                MessageBox.Show("Thông tin đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            NhanVien nv = new NhanVien
            {
                ngayLamViec = tb_date.Text,
                maNV = tb_code.Text,
                loaiLamThem = tb_type.Text,
                soGio = double.Parse(tb_time.Text),
                trangthai = tb_status.Text,
            };
            dataUtils.Add(nv);
            LoadData();
            ClearInputs();
            MessageBox.Show("Thêm mới thành công!");
        }

        string oldNgay = "";
        string oldMa = "";

        private void dtgr_view_SelectionChanged(object sender, EventArgs e)
        {
            if (dtgr_view.CurrentRow == null) return;

            var row = dtgr_view.CurrentRow;

            oldNgay = row.Cells["ngayLamViec"].Value?.ToString();
            oldMa = row.Cells["maNV"].Value?.ToString();

            tb_date.Text = oldNgay;
            tb_code.Text = oldMa;
            tb_type.Text = row.Cells["loaiLamThem"].Value?.ToString();
            tb_time.Text = row.Cells["soGio"].Value?.ToString();
            tb_status.Text = row.Cells["trangthai"].Value?.ToString();
        }


        private void btn_update_Click(object sender, EventArgs e)
        {
            string ngay = tb_date.Text.Trim();
            string ma = tb_code.Text.Trim();

            // Kiểm tra bắt buộc
            if (string.IsNullOrEmpty(ngay) || string.IsNullOrEmpty(ma))
            {
                MessageBox.Show("Trường NGÀY và MÃ NHÂN VIÊN là bắt buộc!",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NhanVien nv = new NhanVien
            {
                ngayLamViec = tb_date.Text,
                maNV = tb_code.Text,
                loaiLamThem = tb_type.Text,
                soGio = double.Parse(tb_time.Text),
                trangthai = tb_status.Text,
            };

            if (!dataUtils.Update(oldNgay, oldMa, nv))
            {
                MessageBox.Show("Thông tin đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadData();
            ClearInputs();
            MessageBox.Show("Cập nhật thành công!");
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(oldNgay) || string.IsNullOrEmpty(oldMa))
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!");
                return;
            }

            if (!dataUtils.Delete(oldNgay, oldMa))
            {
                MessageBox.Show("Không tìm thấy dữ liệu để xóa!");
                return;
            }

            LoadData();
            MessageBox.Show("Đã xóa thành công!");
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            string ngay = tb_date.Text.Trim();
            string ma = tb_code.Text.Trim();

            List<NhanVien> list = dataUtils.Search(ngay, ma);

            dtgr_view.DataSource = list;

            if (list.Count == 0)
                MessageBox.Show("Không tìm thấy kết quả!");
        }

        private void ClearInputs()
        {
            tb_date.Clear();
            tb_code.Clear();
            tb_type.Clear();
            tb_time.Clear();
            tb_status.Clear();
            tb_date.Focus();
        }
    }
}
