import bodyParser from "body-parser";
import express from "express";
import { Dictionary } from "express-serve-static-core";
import { IProduct } from "./models/product";
import { ProductService } from "./productService";
import { ProductStore } from "./productStore";

const app = express();
app.use(bodyParser.json());
const port = process.env.PORT || "8000";
const productService = new ProductService(new ProductStore());

app.get(`/`, async (req, res) => {
    if (isValidParam(req.query)) {
        res.status(200).send(await productService.get(getIdParam(req.query)));
        return;
    }

    res.status(200).send(await productService.getAll());
});

app.post(`/`, async (req, res) => {
    res
        .status(201)
        .send(await productService.create(getProductFromReq(req.body)));
});

app.put(`/`, async (req, res) => {
    if (isValidParam(req.query)) {
        res
            .status(200)
            .send(
                await productService.update(
                    getIdParam(req.query),
                    getProductFromReq(req.body)
                )
            );
        return;
    }

    res.status(400).send();
});

app.delete(`/`, async (req, res) => {
    if (isValidParam(req.query)) {
        res.status(200).send(await productService.delete(getIdParam(req.query)));
        return;
    }

    res.status(400).send();
});

const isValidParam = (params: Dictionary<string>): boolean => {
    const id = getIdParam(params);

    if (id) {
        return true;
    }

    return false;
};

const getIdParam = (params: Dictionary<string>): number => {
    return (params.id as unknown) as number;
};

const getProductFromReq = (body: Dictionary<string>): IProduct => {
    return (body as unknown) as IProduct;
};

app.listen(port);
