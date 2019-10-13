import sqlite3 from "sqlite3";
var Store = /** @class */ (function () {
    function Store() {
        sqlite3.verbose();
    }
    Store.prototype.createDb = function () {
        var _this = this;
        if (this.db) {
            return;
        }
        return new Promise(function (resolve, reject) {
            _this.db = new sqlite3.Database(":memory:", function (err) {
                if (err) {
                    console.error("Failed to create database");
                    reject(err);
                }
                else {
                    console.log("Connected to database. Preparing to seed");
                    resolve();
                }
            });
        });
    };
    Store.prototype.run = function (sql, params) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.db.get(sql, params, function (err, result) {
                if (err) {
                    console.error(err);
                    reject(err);
                }
                else {
                    resolve(result);
                }
            });
        });
    };
    Store.prototype.all = function (sql, params) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.db.all(sql, params, function (err, result) {
                if (err) {
                    console.error(err);
                    reject(err);
                }
                else {
                    resolve(result);
                }
            });
        });
    };
    Store.prototype.get = function (sql, params) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.db.get(sql, params, function (err, result) {
                if (err) {
                    console.error(err);
                    reject(err);
                }
                else {
                    resolve(result);
                }
            });
        });
    };
    Store.prototype.runNoResult = function (sql, params) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.db.run(sql, params, function (err) {
                if (err) {
                    console.error(err);
                    reject(err);
                }
                else {
                    resolve();
                }
            });
        });
    };
    return Store;
}());
export default Store;
//# sourceMappingURL=store.js.map