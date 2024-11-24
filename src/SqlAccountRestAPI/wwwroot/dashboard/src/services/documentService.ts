import msgpack from "msgpack-lite";
import {useServiceEndpoint} from "@/utils/serviceEndpoint";

export interface BizObjectEntry {
  name: string;
}
export interface BizObjectDetail {
  name: string;
  dataset: BizObjectDataset[]
}
export interface BizObjectDataset {
  name: string;
  fields: string[]
}

const serviceEndpointHandler = useServiceEndpoint()
/**
 * Fetch BizObjects.
 */
export const fetchBizObject = async (): Promise<BizObjectEntry[]> => {
  const apiUrl = serviceEndpointHandler.normalize(`api/app/objects`);
  const response = await fetch(apiUrl, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) {
    throw new Error(`Error fetching BizObject list: ${response.statusText}`);
  }

  const data: BizObjectEntry[] = await response.json();
  // Handle duplicate names
  const uniqueData = Array.from(
    new Map(data.map(item => [item.name, item])).values()
  );
  return uniqueData;
};

export const fetchBizObjectDetail = async (name: string): Promise<BizObjectDetail> => {
  const apiUrl = serviceEndpointHandler.normalize(`api/app/objects/${name}`);
  const response = await fetch(apiUrl, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) { 
    throw new Error(`Failed to fetch BizObject detail: ${response.statusText}`);
  }

  const data = await response.json();
  return data;
};