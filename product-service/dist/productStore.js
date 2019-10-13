var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
import { Product } from "./models/product";
import Store from "./store";
var ProductStore = /** @class */ (function () {
    function ProductStore() {
        var _this = this;
        this.products = [
            new Product(1, "Harry Potter and the Philosopher's Stone", "First HP book by JK Rowling", 14.99),
            new Product(2, "Harry Potter and the Chamber of Secrets", "More adventures from Harry and pals", 13.99)
        ];
        this.createTable = function () {
            console.log("Creating Table");
            var query = _this.store.db.prepare("CREATE TABLE IF NOT EXISTS Product(id INTEGER PRIMARY KEY ASC, name TEXT, description TEXT, price NUMERIC)");
            query.run(function (err) {
                if (err) {
                    console.log("create:" + JSON.stringify(err));
                }
                else {
                    console.log("seeding data");
                    _this.seedData();
                }
            });
        };
        this.seedData = function () {
            _this.products.map(function (p) {
                var query = _this.store.db.prepare("INSERT INTO Product (name, description, price) VALUES (?,?,?)", function (err) {
                    if (err) {
                        return console.log("Seeding: " + JSON.stringify(err));
                    }
                    return console.log("seeding complete");
                });
                query.run(p.name, p.description, p.price);
            });
        };
        this.store = new Store();
        this.store.createDb().then(function () {
            _this.createTable();
        });
    }
    ProductStore.prototype.get = function (id) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.store.get("SELECT * FROM Product WHERE id = ?;", [id])];
                    case 1: return [2 /*return*/, _a.sent()];
                }
            });
        });
    };
    ProductStore.prototype.getAll = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.store
                            .all("SELECT * FROM Product;", [])
                            .catch(function (err) {
                            console.log(err);
                            return [];
                        })];
                    case 1: return [2 /*return*/, _a.sent()];
                }
            });
        });
    };
    ProductStore.prototype.create = function (product) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.store.run("INSERT INTO Product(name, description, price) VALUES (?,?,?);", [product.name, product.description, product.price])];
                    case 1:
                        _a.sent();
                        return [4 /*yield*/, this.store.run("SELECT last_insert_rowid() as id;", [])];
                    case 2: return [2 /*return*/, _a.sent()];
                }
            });
        });
    };
    ProductStore.prototype.update = function (id, product) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.store.runNoResult("UPDATE Product\n            SET name = ?,\n                description = ?,\n                price = ?\n            WHERE id = ?;", [product.name, product.description, product.price, id])
                            .catch(function (err) {
                            console.error(err);
                            return null;
                        })];
                    case 1:
                        _a.sent();
                        return [4 /*yield*/, this.get(id)];
                    case 2: return [2 /*return*/, _a.sent()];
                }
            });
        });
    };
    ProductStore.prototype.delete = function (id) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.store.runNoResult("DELETE FROM Product WHERE id = ?;", [id])];
                    case 1:
                        _a.sent();
                        return [2 /*return*/];
                }
            });
        });
    };
    return ProductStore;
}());
export { ProductStore };
//# sourceMappingURL=productStore.js.map