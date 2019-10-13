import sqlite3, { Database, Statement } from "sqlite3";

export default class Store {
    public db: Database;
    constructor() {
        sqlite3.verbose();
    }

    public createDb(): Promise<void> {
        if (this.db) {
            return;
        }

        return new Promise((resolve, reject) => {
            this.db = new sqlite3.Database(":memory:", (err) => {
                if (err) {
                    console.error("Failed to create database");
                    reject(err);
                } else {
                    console.log("Connected to database. Preparing to seed");
                    resolve();
                }
            });
        });
    }

    public run(sql: string, params: any[]): Promise<number> {
        return new Promise((resolve, reject) => {
            this.db.get(sql, params, (err, result) => {
                if (err) {
                    console.error(err);
                    reject(err);
                } else {
                    resolve(result);
                }
            });
        });
    }

    public all<T>(sql: string, params: any[]): Promise<T[]> {
        return new Promise((resolve, reject) => {
            this.db.all(sql, params, (err, result) => {
                if (err) {
                    console.error(err);
                    reject(err);
                } else {
                    resolve(result as T[]);
                }
            });
        });
    }

    public get<T>(sql: string, params: any[]): Promise<T> {
        return new Promise((resolve, reject) => {
            this.db.get(sql, params, (err, result) => {
                if (err) {
                    console.error(err);
                    reject(err);
                } else {
                    resolve(result as T);
                }
            });
        });
    }

    public runNoResult(sql: string, params: any[]): Promise<void> {
        return new Promise((resolve, reject) => {
            this.db.run(sql, params, (err) => {
                if (err) {
                    console.error(err);
                    reject(err);
                } else {
                    resolve();
                }
            });
        });
    }
}
