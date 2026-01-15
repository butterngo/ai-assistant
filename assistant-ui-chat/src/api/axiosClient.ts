import axios from "axios";
import { API_BASE } from "../config";

export const axiosClient = axios.create({
  baseURL: API_BASE,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});