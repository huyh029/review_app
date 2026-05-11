# Prompt mô tả dự án: Review UI (Hệ thống đánh giá cán bộ)

## Tổng quan

Đây là ứng dụng web Angular (standalone components, Angular 14+) dùng để quản lý quy trình đánh giá cán bộ theo kỳ (tháng/năm). Hệ thống hỗ trợ 3 vai trò chính: cán bộ tự đánh giá, lãnh đạo chấm điểm, và xem kết quả tổng hợp. Ngoài ra có phân hệ cấu hình dành cho quản trị viên.

---

## Cấu trúc thư mục

```
review-ui/
└── src/app/
    ├── app.ts                        # Root component
    ├── app.routes.ts                 # Định nghĩa toàn bộ routes
    ├── app.config.ts                 # Providers toàn cục (HttpClient, interceptor, router)
    ├── app.html / app.css            # Layout chính (sidebar + header + router-outlet + pagination)
    ├── interceptors/
    │   └── auth.interceptor.ts       # Gắn Bearer token, xử lý lỗi HTTP toàn cục
    ├── services/
    │   ├── toast.service.ts          # Thông báo toast (signal-based)
    │   ├── pagination.service.ts     # Quản lý trạng thái phân trang (BehaviorSubject)
    │   ├── sidebar.service.ts        # Trạng thái đóng/mở sidebar
    │   └── page-title.service.ts     # Tiêu đề trang động
    ├── components/
    │   └── toast/toast.component.ts  # Component hiển thị toast toàn cục
    └── page/
        ├── auth/
        │   ├── login/                # Trang đăng nhập
        │   └── services/auth.service.ts
        ├── protected/
        │   ├── home/                 # Trang chủ (dashboard)
        │   ├── evaluation-board/     # Bảng đánh giá (self / manager / result)
        │   │   ├── detail/           # Trang chi tiết / tạo mới / chỉnh sửa phiếu
        │   │   ├── self/             # Tab tự đánh giá
        │   │   ├── manager/          # Tab lãnh đạo chấm điểm
        │   │   ├── result/           # Tab kết quả
        │   │   └── evaluation-board.service.ts
        │   ├── reports/              # Trang báo cáo tổng hợp
        │   └── configuration/
        │       ├── evaluation-object/ # Cấu hình đối tượng đánh giá + vai trò
        │       ├── evaluation-flow/   # Cấu hình luồng đánh giá
        │       ├── evaluation-criteria/ # Cấu hình bộ tiêu chí
        │       ├── report-type/       # Cấu hình loại báo cáo
        │       └── api-access/        # Cấu hình API access
        └── component/                 # Shared UI components dùng lại nhiều nơi
            ├── table/
            ├── filter/
            ├── select/
            ├── header/
            ├── siderbar/
            ├── pagination-footer/
            ├── comment-section/
            ├── emoji-picker/
            ├── confirmation-dialog/
            ├── criteria-table/
            ├── evaluation-tree/
            ├── download-button/
            ├── dropdown-select/
            └── department-selection/
```

---

## Danh sách Pages (Routes)

| Route | Component | Mô tả |
|---|---|---|
| `/login` | LoginComponent | Đăng nhập bằng userId |
| `/home` | HomeComponent | Trang chủ / dashboard |
| `/evaluation-board` | EvaluationBoardComponent | Bảng đánh giá (mặc định tab self) |
| `/evaluation-board/:type` | EvaluationBoardComponent | Tab: `self` / `manager` / `result` |
| `/evaluation-board/:type/detail/:id` | EvaluationBoardDetailComponent | Chi tiết / tạo mới / chỉnh sửa phiếu |
| `/reports` | ReportsComponent | Báo cáo tổng hợp |
| `/configuration/evaluation-object` | EvaluationObjectComponent | Cấu hình đối tượng (có 2 sub-tab) |
| `/configuration/evaluation-object/configuration` | ConfigurationComponent | Sub-tab cấu hình đối tượng |
| `/configuration/evaluation-object/role` | RoleComponent | Sub-tab quản lý vai trò |
| `/configuration/evaluation-flow` | EvaluationFlowComponent | Danh sách luồng đánh giá |
| `/configuration/evaluation-flow/new` | EvaluationFlowDetailComponent | Tạo mới luồng |
| `/configuration/evaluation-flow/:code/edit` | EvaluationFlowDetailComponent | Chỉnh sửa luồng |
| `/configuration/evaluation-criteria` | EvaluationCriteriaComponent | Danh sách bộ tiêu chí |
| `/configuration/evaluation-criteria/new` | EvaluationCriteriaDetailComponent | Tạo mới bộ tiêu chí |
| `/configuration/evaluation-criteria/:id/edit` | EvaluationCriteriaDetailComponent | Chỉnh sửa bộ tiêu chí |
| `/configuration/report-type` | ReportTypeComponent | Danh sách loại báo cáo |
| `/configuration/report-type/new` | ReportTypeDetailComponent | Tạo mới loại báo cáo |
| `/configuration/report-type/:id/edit` | ReportTypeDetailComponent | Chỉnh sửa loại báo cáo |
| `/configuration/api-access` | ApiAccessComponent | Quản lý API access |

**Tổng: 19 routes, tương ứng ~12 page component riêng biệt.**

---

## Chi tiết chức năng từng Page

### 1. Login (`/login`)
- Nhập userId để đăng nhập
- Gọi `POST /api/auth/login`
- Lưu token + thông tin user vào localStorage
- Redirect về `/home` sau khi đăng nhập thành công

### 2. Home (`/home`)
- Trang chủ đơn giản, hiển thị dashboard tổng quan

### 3. Evaluation Board (`/evaluation-board/:type`)
- 3 tab: **Tự đánh giá** (`self`), **Lãnh đạo chấm** (`manager`), **Kết quả** (`result`)
- Mỗi tab có:
  - Bộ lọc: tên, kỳ (tháng), năm, trạng thái, đơn vị
  - Bảng danh sách có phân trang
  - Multi-select với xóa nhiều (isAll + excludeIds)
  - Nút thêm mới (chỉ tab self)
  - Click vào dòng → vào trang chi tiết

### 4. Evaluation Board Detail (`/evaluation-board/:type/detail/:id`)
Trang phức tạp nhất, xử lý 3 chế độ:
- **Tạo mới** (`id = new`): load template từ API theo tháng/năm, chọn bộ tiêu chí
- **Chỉnh sửa** (có id): load dữ liệu phiếu hiện tại
- **Xem kết quả** (type = result): chỉ đọc

Chức năng:
- Hiển thị cây tiêu chí phân cấp (cha/con), tính điểm tổng tự động
- Nhập điểm tự chấm (self) hoặc điểm lãnh đạo (manager)
- Tính điểm cha = tổng điểm con (hỗ trợ tiêu chí trừ điểm)
- Phân loại kết quả theo bảng xếp loại (min/max score)
- Các nút hành động theo trạng thái phiếu:
  - `draft`: Lưu, Gửi lãnh đạo, Xóa
  - `pending`: Thu hồi
  - `pending_director`: Lưu điểm lãnh đạo
  - `completed`: Xem kết quả
- Modal chọn lãnh đạo khi gửi phiếu (có tìm kiếm)
- Section bình luận: gửi comment, reply, emoji reaction, upload file
- Trình chỉnh sửa ảnh (canvas-based): crop, vẽ (pen/line/rectangle/circle/arrow), điều chỉnh brightness/contrast/saturation/rotation

### 5. Reports (`/reports`)
- Bộ lọc: loại báo cáo, kỳ (tháng), năm
- Bảng kết quả với cột nhóm (grouped headers):
  - Cán bộ, Đơn vị
  - Đánh giá: điểm tự chấm / điểm lãnh đạo
  - Kết quả phân loại: tự phân loại / phân loại tại đơn vị
- Phân trang
- Nút tải xuống báo cáo

### 6. Configuration - Evaluation Object (`/configuration/evaluation-object`)
- 2 sub-tab: **Cấu hình đối tượng** và **Vai trò**
- CRUD đối tượng đánh giá và vai trò

### 7. Configuration - Evaluation Flow (`/configuration/evaluation-flow`)
- Danh sách luồng đánh giá, có lọc, phân trang, xóa nhiều
- Trang detail: tạo mới / chỉnh sửa luồng

### 8. Configuration - Evaluation Criteria (`/configuration/evaluation-criteria`)
- Danh sách bộ tiêu chí, có lọc, phân trang, xóa nhiều
- Trang detail: tạo mới / chỉnh sửa bộ tiêu chí (cây tiêu chí phân cấp)

### 9. Configuration - Report Type (`/configuration/report-type`)
- Danh sách loại báo cáo, có lọc, phân trang, xóa nhiều
- Trang detail: tạo mới / chỉnh sửa loại báo cáo

### 10. Configuration - API Access (`/configuration/api-access`)
- Quản lý cấu hình API access key / token

---

## Shared Components

| Component | Chức năng |
|---|---|
| `TableComponent` | Bảng dữ liệu: multi-select (isAll/includeIds/excludeIds), grouped headers, icon actions, badge status, sanitize HTML |
| `FilterComponent` | Bộ lọc động: text search + select dropdown, hỗ trợ realtime hoặc click |
| `SelectComponent` | Dropdown tùy chỉnh, implement ControlValueAccessor |
| `HeaderComponent` | Thanh header: thông tin user, nút logout |
| `SiderbarComponent` | Sidebar trái: menu điều hướng, submenu cấu hình, toggle đóng/mở |
| `PaginationFooterComponent` | Footer phân trang: chọn số dòng/trang, chuyển trang |
| `ToastComponent` | Hiển thị toast notification (success/error/warning/info) |
| `ConfirmationDialogComponent` | Dialog xác nhận xóa |
| `CommentSectionComponent` | Bình luận: gửi/xóa comment, reply, emoji reaction, upload file, xem ảnh |
| `EmojiPickerComponent` | Bộ chọn emoji cho reaction |
| `EvaluationTreeComponent` | Hiển thị cây tiêu chí phân cấp |
| `CriteriaTableComponent` | Bảng tiêu chí |
| `DownloadButtonComponent` | Nút tải xuống / export |
| `DropdownSelectComponent` | Dropdown có tìm kiếm (dùng cho chọn lãnh đạo) |
| `DepartmentSelectionComponent` | Chọn đơn vị/phòng ban |

---

## Services

| Service | Chức năng |
|---|---|
| `AuthService` | Login, logout, lưu/lấy token và user từ localStorage |
| `EvaluationBoardService` | Toàn bộ nghiệp vụ phiếu đánh giá: CRUD self/manager/result, submit, recall, approve, complete, comment, reaction, upload file |
| `ReportsService` | Lấy dữ liệu báo cáo, lấy danh sách loại báo cáo |
| `EvaluationFlowService` | CRUD luồng đánh giá |
| `EvaluationCriteriaService` | CRUD bộ tiêu chí |
| `ReportTypeService` | CRUD loại báo cáo |
| `ClassificationService` | Quản lý bảng xếp loại |
| `PaginationService` | State phân trang toàn cục (BehaviorSubject) |
| `SidebarService` | State đóng/mở sidebar |
| `ToastService` | Hiển thị toast (signal-based, auto-dismiss) |
| `PageTitleService` | Tiêu đề trang động |

---

## Interceptor & Auth

- `authInterceptor`: Tự động gắn `Authorization: Bearer <token>` vào mọi request HTTP
- Xử lý lỗi tập trung:
  - `401` → Xóa auth, redirect `/login`, toast "Phiên đăng nhập hết hạn"
  - `403` → Toast "Không có quyền"
  - `404` → Toast "Không tìm thấy dữ liệu"
  - `500` → Toast "Lỗi máy chủ"
  - Các lỗi 4xx khác → Toast message từ response

---

## API Endpoints

### Auth
- `POST /api/auth/login`
- `POST /api/auth/logout`

### Evaluation Board
- `GET/POST /api/page/evaluation-board/self`
- `GET/PUT/DELETE /api/page/evaluation-board/self/:id`
- `POST /api/page/evaluation-board/self/:id/submit`
- `POST /api/page/evaluation-board/self/:id/recall`
- `DELETE /api/page/evaluation-board/self` (batch)
- `GET /api/page/evaluation-board/self/template`
- `POST /api/page/evaluation-board/self/save-and-submit`
- `GET/POST /api/page/evaluation-board/manager`
- `GET /api/page/evaluation-board/manager/:id`
- `POST /api/page/evaluation-board/manager/:id/review`
- `POST /api/page/evaluation-board/manager/:id/approve`
- `POST /api/page/evaluation-board/manager/:id/update-scores`
- `POST /api/page/evaluation-board/manager/:id/complete`
- `GET /api/page/evaluation-board/result`
- `GET /api/page/evaluation-board/result/:id`
- `GET /api/page/evaluation-board/managers`
- `GET/POST /api/page/evaluation-board/comments/:id`
- `DELETE /api/page/evaluation-board/comments/:id`
- `POST /api/page/evaluation-board/comments/:id/reactions`
- `DELETE /api/page/evaluation-board/comments/:id/reactions/:userId`
- `POST /api/page/evaluation-board/comments/:id/files`

### Configuration
- `GET/POST /api/page/configuration/evaluation-flow`
- `PUT/DELETE /api/page/configuration/evaluation-flow/:code`
- `DELETE /api/page/configuration/evaluation-flow` (batch)
- `GET/POST /api/page/configuration/evaluation-criteria`
- `PUT/DELETE /api/page/configuration/evaluation-criteria/:id`
- `DELETE /api/page/configuration/evaluation-criteria` (batch)
- `GET/POST /api/page/configuration/report-type`
- `PUT/DELETE /api/page/configuration/report-type/:code`
- `DELETE /api/page/configuration/report-type` (batch)

### Reports
- `GET /api/page/reports`
- `GET /api/page/reports/report-type-options`

---

## Các pattern kiến trúc nổi bật

- **Standalone components** (không dùng NgModule)
- **ChangeDetectionStrategy.OnPush** trên các component nặng
- **BehaviorSubject** cho state phân trang và sidebar
- **Signal** cho toast notifications
- **Multi-select với isAll + excludeIds**: cho phép "chọn tất cả" mà không cần load toàn bộ dữ liệu
- **Hierarchical criteria scoring**: điểm cha = tổng điểm con, hỗ trợ tiêu chí trừ điểm, giới hạn maxScore
- **Canvas-based image editor**: crop, vẽ hình, điều chỉnh filter ảnh trực tiếp trong trình duyệt
- **Proxy config** (`proxy.conf.json`): forward `/api/*` sang backend trong dev
- **Tailwind CSS** (postcss.config.js) cho styling
- **Lucide Angular** cho icon set
