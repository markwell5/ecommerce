import { gql } from '@apollo/client';

export const GET_PRODUCTS = gql`
  query GetProducts($page: Int!, $pageSize: Int!) {
    products(page: $page, pageSize: $pageSize) {
      items {
        id
        name
        description
        category
        price
        stockLevel {
          availableQuantity
          reservedQuantity
        }
      }
      totalCount
      page
      pageSize
    }
  }
`;

export const GET_PRODUCT = gql`
  query GetProduct($id: Long!) {
    product(id: $id) {
      id
      name
      description
      category
      price
      stockLevel {
        availableQuantity
        reservedQuantity
      }
    }
  }
`;

export const CREATE_PRODUCT = gql`
  mutation CreateProduct($name: String!, $description: String!, $category: String!, $price: Decimal!) {
    createProduct(name: $name, description: $description, category: $category, price: $price) {
      id
      name
      description
      category
      price
    }
  }
`;

export const UPDATE_PRODUCT = gql`
  mutation UpdateProduct($id: Long!, $name: String!, $description: String!, $category: String!, $price: Decimal!) {
    updateProduct(id: $id, name: $name, description: $description, category: $category, price: $price) {
      id
      name
      description
      category
      price
    }
  }
`;

export const DELETE_PRODUCT = gql`
  mutation DeleteProduct($id: Long!) {
    deleteProduct(id: $id)
  }
`;

export const UPDATE_STOCK = gql`
  mutation UpdateStock($productId: Long!, $quantity: Int!) {
    updateStock(productId: $productId, quantity: $quantity) {
      productId
      availableQuantity
      reservedQuantity
    }
  }
`;

export const GET_CATEGORIES = gql`
  query GetCategories {
    categories {
      id
      name
      slug
    }
  }
`;
