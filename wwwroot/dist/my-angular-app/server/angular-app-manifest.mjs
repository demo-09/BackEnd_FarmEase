
export default {
  bootstrap: () => import('./main.server.mjs').then(m => m.default),
  inlineCriticalCss: true,
  baseHref: '/',
  locale: undefined,
  routes: [
  {
    "renderMode": 2,
    "redirectTo": "/HomePage",
    "route": "/"
  },
  {
    "renderMode": 2,
    "route": "/HomePage"
  },
  {
    "renderMode": 2,
    "route": "/About"
  },
  {
    "renderMode": 2,
    "route": "/Products"
  },
  {
    "renderMode": 2,
    "route": "/Cart"
  },
  {
    "renderMode": 2,
    "route": "/Wishlist"
  },
  {
    "renderMode": 2,
    "route": "/ai"
  },
  {
    "renderMode": 2,
    "route": "/Profile"
  },
  {
    "renderMode": 2,
    "route": "/Orders"
  },
  {
    "renderMode": 2,
    "route": "/Contact"
  },
  {
    "renderMode": 2,
    "route": "/Admin"
  },
  {
    "renderMode": 2,
    "route": "/Login"
  },
  {
    "renderMode": 2,
    "route": "/Signup"
  },
  {
    "renderMode": 2,
    "route": "/DataComponent"
  },
  {
    "renderMode": 2,
    "route": "/Chat"
  },
  {
    "renderMode": 2,
    "route": "/News"
  },
  {
    "renderMode": 2,
    "route": "/AddProduct"
  },
  {
    "renderMode": 0,
    "route": "/product-detail/*/*"
  },
  {
    "renderMode": 2,
    "route": "/order-detail"
  },
  {
    "renderMode": 2,
    "route": "/Weather"
  },
  {
    "renderMode": 2,
    "redirectTo": "/HomePage",
    "route": "/**"
  }
],
  entryPointToBrowserMapping: undefined,
  assets: {
    'index.csr.html': {size: 42892, hash: '476a05ebf103102c7463262242cd0d36a19d6be0bc675ede5146fa8e21dec170', text: () => import('./assets-chunks/index_csr_html.mjs').then(m => m.default)},
    'index.server.html': {size: 23886, hash: '72bd5139b9c01db4fa7ce5f642c0090c917f7aa23bebd20303e42d16b9e3d326', text: () => import('./assets-chunks/index_server_html.mjs').then(m => m.default)},
    'ai/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/ai_index_html.mjs').then(m => m.default)},
    'HomePage/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/HomePage_index_html.mjs').then(m => m.default)},
    'DataComponent/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/DataComponent_index_html.mjs').then(m => m.default)},
    'Login/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Login_index_html.mjs').then(m => m.default)},
    'Profile/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Profile_index_html.mjs').then(m => m.default)},
    'About/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/About_index_html.mjs').then(m => m.default)},
    'order-detail/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/order-detail_index_html.mjs').then(m => m.default)},
    'News/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/News_index_html.mjs').then(m => m.default)},
    'Products/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Products_index_html.mjs').then(m => m.default)},
    'Wishlist/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Wishlist_index_html.mjs').then(m => m.default)},
    'Signup/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Signup_index_html.mjs').then(m => m.default)},
    'Contact/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Contact_index_html.mjs').then(m => m.default)},
    'AddProduct/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/AddProduct_index_html.mjs').then(m => m.default)},
    'Cart/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Cart_index_html.mjs').then(m => m.default)},
    'Weather/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Weather_index_html.mjs').then(m => m.default)},
    'Chat/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Chat_index_html.mjs').then(m => m.default)},
    'Orders/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Orders_index_html.mjs').then(m => m.default)},
    'Admin/index.html': {size: 48412, hash: 'c9a503033334be75b5314188fe671c0df8aaca10e69e618f0d25a59e9709467e', text: () => import('./assets-chunks/Admin_index_html.mjs').then(m => m.default)},
    'styles-NBK5NOIS.css': {size: 37620, hash: 'AH2ZAPEq9EQ', text: () => import('./assets-chunks/styles-NBK5NOIS_css.mjs').then(m => m.default)}
  },
};
