import { useState } from 'react';
import { useQuery, useMutation } from '@apollo/client/react';
import {
  Box,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TablePagination,
  IconButton,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  Chip,
  InputAdornment,
  CircularProgress,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Search as SearchIcon,
  Inventory as StockIcon,
} from '@mui/icons-material';
import {
  GET_PRODUCTS,
  CREATE_PRODUCT,
  UPDATE_PRODUCT,
  DELETE_PRODUCT,
  UPDATE_STOCK,
} from '../graphql/products';

interface Product {
  id: number;
  name: string;
  description: string;
  category: string;
  price: number;
  stockLevel?: {
    availableQuantity: number;
    reservedQuantity: number;
  };
}

interface ProductFormData {
  name: string;
  description: string;
  category: string;
  price: string;
}

const emptyForm: ProductFormData = { name: '', description: '', category: '', price: '' };

export default function ProductsPage() {
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');

  // Dialogs
  const [formOpen, setFormOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [formData, setFormData] = useState<ProductFormData>(emptyForm);
  const [formError, setFormError] = useState('');

  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null);

  const [stockTarget, setStockTarget] = useState<Product | null>(null);
  const [stockQuantity, setStockQuantity] = useState('');

  // Queries & mutations
  const { data, loading, error, refetch } = useQuery(GET_PRODUCTS, {
    variables: { page: page + 1, pageSize },
    fetchPolicy: 'cache-and-network',
  });

  const [createProduct, { loading: creating }] = useMutation(CREATE_PRODUCT);
  const [updateProduct, { loading: updating }] = useMutation(UPDATE_PRODUCT);
  const [deleteProduct, { loading: deleting }] = useMutation(DELETE_PRODUCT);
  const [updateStock, { loading: updatingStock }] = useMutation(UPDATE_STOCK);

  const products: Product[] = data?.products?.items ?? [];
  const totalCount: number = data?.products?.totalCount ?? 0;

  const filteredProducts = search
    ? products.filter(
        (p) =>
          p.name.toLowerCase().includes(search.toLowerCase()) ||
          p.category.toLowerCase().includes(search.toLowerCase())
      )
    : products;

  // Form handlers
  const openCreate = () => {
    setEditingProduct(null);
    setFormData(emptyForm);
    setFormError('');
    setFormOpen(true);
  };

  const openEdit = (product: Product) => {
    setEditingProduct(product);
    setFormData({
      name: product.name,
      description: product.description,
      category: product.category,
      price: String(product.price),
    });
    setFormError('');
    setFormOpen(true);
  };

  const handleFormSubmit = async () => {
    const { name, description, category, price } = formData;
    if (!name || !description || !category || !price) {
      setFormError('All fields are required');
      return;
    }
    const priceNum = parseFloat(price);
    if (isNaN(priceNum) || priceNum <= 0) {
      setFormError('Price must be a positive number');
      return;
    }

    try {
      if (editingProduct) {
        await updateProduct({
          variables: { id: editingProduct.id, name, description, category, price: priceNum },
        });
      } else {
        await createProduct({
          variables: { name, description, category, price: priceNum },
        });
      }
      setFormOpen(false);
      refetch();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Operation failed');
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await deleteProduct({ variables: { id: deleteTarget.id } });
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      console.error('Delete failed:', err);
    }
  };

  const handleStockUpdate = async () => {
    if (!stockTarget) return;
    const qty = parseInt(stockQuantity, 10);
    if (isNaN(qty) || qty < 0) return;
    try {
      await updateStock({ variables: { productId: stockTarget.id, quantity: qty } });
      setStockTarget(null);
      setStockQuantity('');
      refetch();
    } catch (err) {
      console.error('Stock update failed:', err);
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Products</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>
          Add Product
        </Button>
      </Box>

      <TextField
        placeholder="Search by name or category..."
        size="small"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        sx={{ mb: 2, width: 350 }}
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          },
        }}
      />

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to load products: {error.message}
        </Alert>
      )}

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Name</TableCell>
              <TableCell>Category</TableCell>
              <TableCell align="right">Price</TableCell>
              <TableCell align="right">Stock</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading && !data ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                  <CircularProgress />
                </TableCell>
              </TableRow>
            ) : filteredProducts.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">No products found</Typography>
                </TableCell>
              </TableRow>
            ) : (
              filteredProducts.map((product) => (
                <TableRow key={product.id} hover>
                  <TableCell>{product.id}</TableCell>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {product.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {product.description.length > 80
                        ? product.description.slice(0, 80) + '...'
                        : product.description}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={product.category} size="small" />
                  </TableCell>
                  <TableCell align="right">${product.price.toFixed(2)}</TableCell>
                  <TableCell align="right">
                    <Chip
                      label={product.stockLevel?.availableQuantity ?? '—'}
                      size="small"
                      color={
                        (product.stockLevel?.availableQuantity ?? 0) > 10
                          ? 'success'
                          : (product.stockLevel?.availableQuantity ?? 0) > 0
                            ? 'warning'
                            : 'error'
                      }
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => openEdit(product)} title="Edit">
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => {
                        setStockTarget(product);
                        setStockQuantity(
                          String(product.stockLevel?.availableQuantity ?? 0)
                        );
                      }}
                      title="Adjust stock"
                    >
                      <StockIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      color="error"
                      onClick={() => setDeleteTarget(product)}
                      title="Delete"
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={(_, newPage) => setPage(newPage)}
          rowsPerPage={pageSize}
          onRowsPerPageChange={(e) => {
            setPageSize(parseInt(e.target.value, 10));
            setPage(0);
          }}
          rowsPerPageOptions={[5, 10, 25, 50]}
        />
      </TableContainer>

      {/* Create/Edit Dialog */}
      <Dialog open={formOpen} onClose={() => setFormOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingProduct ? 'Edit Product' : 'Create Product'}</DialogTitle>
        <DialogContent>
          {formError && (
            <Alert severity="error" sx={{ mb: 2, mt: 1 }}>
              {formError}
            </Alert>
          )}
          <TextField
            label="Name"
            fullWidth
            required
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            sx={{ mt: 1, mb: 2 }}
          />
          <TextField
            label="Description"
            fullWidth
            required
            multiline
            rows={3}
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            sx={{ mb: 2 }}
          />
          <TextField
            label="Category"
            fullWidth
            required
            value={formData.category}
            onChange={(e) => setFormData({ ...formData, category: e.target.value })}
            sx={{ mb: 2 }}
          />
          <TextField
            label="Price"
            fullWidth
            required
            type="number"
            value={formData.price}
            onChange={(e) => setFormData({ ...formData, price: e.target.value })}
            slotProps={{
              input: {
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              },
            }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFormOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleFormSubmit}
            disabled={creating || updating}
          >
            {creating || updating ? 'Saving...' : editingProduct ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)}>
        <DialogTitle>Delete Product</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete <strong>{deleteTarget?.name}</strong>? This action
            cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteTarget(null)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleDelete}
            disabled={deleting}
          >
            {deleting ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Stock Adjustment Dialog */}
      <Dialog open={!!stockTarget} onClose={() => setStockTarget(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Adjust Stock — {stockTarget?.name}</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2, mt: 1 }}>
            Current stock: {stockTarget?.stockLevel?.availableQuantity ?? 0} available,{' '}
            {stockTarget?.stockLevel?.reservedQuantity ?? 0} reserved
          </Typography>
          <TextField
            label="New quantity"
            type="number"
            fullWidth
            value={stockQuantity}
            onChange={(e) => setStockQuantity(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStockTarget(null)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleStockUpdate}
            disabled={updatingStock}
          >
            {updatingStock ? 'Updating...' : 'Update Stock'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
