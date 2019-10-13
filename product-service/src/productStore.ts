import { IProduct, Product } from "./models/product";
import Store from "./store";

export interface IProductStore {
    get(id: number): Promise<IProduct>;
    getAll(): Promise<IProduct[]>;
    create(product: IProduct): Promise<number>;
    update(id: number, product: IProduct): Promise<IProduct>;
    delete(id: number): Promise<void>;
}

export class ProductStore implements IProductStore {
    private products: IProduct[] = [
        new Product(
            1,
            "Harry Potter and the Philosopher's Stone",
            "First HP book by JK Rowling",
            14.99
        ),
        new Product(
            2,
            "Harry Potter and the Chamber of Secrets",
            "More adventures from Harry and pals",
            13.99
        )
    ];

    private store: Store;

    constructor() {
        this.store = new Store();
        this.store.createDb().then(() => {
            this.createTable();
        });
    }

    public async get(id: number): Promise<IProduct> {
        return await this.store.get<IProduct>(`SELECT * FROM Product WHERE id = ?;`, [id]);
    }

    public async getAll(): Promise<IProduct[]> {
        try {
            return await this.store
                .all<IProduct>(`SELECT * FROM Product;`, []);
        } catch (err) {
            console.log(err);
            return [];
        }
    }

    public async create(product: IProduct): Promise<number> {
        try {
            await this.store.run(`INSERT INTO Product(name, description, price) VALUES (?,?,?);`,
                [product.name, product.description, product.price]);

            return await this.store.run(`SELECT last_insert_rowid() as id;`, []);
        } catch (err) {
            console.error(err);
            return 0;
        }
    }

    public async update(id: number, product: IProduct): Promise<IProduct> {
        try {
            await this.store.runNoResult(`UPDATE Product
            SET name = ?,
                description = ?,
                price = ?
            WHERE id = ?;`,
                [product.name, product.description, product.price, id]);

            return await this.get(id);
        } catch (err) {
            console.error(err);
            return null;
        }
    }

    public async delete(id: number): Promise<void> {
        try {
            await this.store.runNoResult(`DELETE FROM Product WHERE id = ?;`, [id]);
        } catch (err) {
            console.error(err);
        }
    }

    private createTable = (): void => {
        console.log("Creating Table");
        const query = this.store.db.prepare(
            "CREATE TABLE IF NOT EXISTS Product(id INTEGER PRIMARY KEY ASC, name TEXT, description TEXT, price NUMERIC)"
        );
        query.run((err) => {
            if (err) {
                console.log("create:" + JSON.stringify(err));
            } else {
                console.log("seeding data");
                this.seedData();
            }
        });
    }

    private seedData = (): void => {
        this.products.map((p) => {
            const query = this.store.db.prepare(
                "INSERT INTO Product (name, description, price) VALUES (?,?,?)",
                (err) => {
                    if (err) {
                        return console.log("Seeding: " + JSON.stringify(err));
                    }
                    return console.log("seeding complete");
                }
            );
            query.run(p.name, p.description, p.price);
        });
    }
}
