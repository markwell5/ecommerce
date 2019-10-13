import { IProduct } from "./models/product";
import { IProductStore } from "./productStore";

export interface IProductService {
    get(id: number): Promise<IProduct>;
    getAll(): Promise<IProduct[]>;
    create(product: IProduct): Promise<number>;
    update(id: number, product: IProduct): Promise<IProduct>;
    delete(id: number): Promise<void>;
}

export class ProductService implements IProductService {
    private productStore: IProductStore;

    constructor(store: IProductStore) {
        this.productStore = store;
    }

    public async getAll(): Promise<IProduct[]> {
        const products = await this.productStore.getAll();

        return products;
    }

    public async get(id: number): Promise<IProduct> {
        const product = await this.productStore.get(id);

        return product;
    }

    public async create(product: IProduct): Promise<number> {
        const newId = await this.productStore.create(product);

        return newId;
    }

    public async update(id: number, product: IProduct): Promise<IProduct> {
        const updated = await this.productStore.update(id, product);

        return updated;
    }

    public async delete(id: number): Promise<void> {
        await this.productStore.delete(id);
    }
}
