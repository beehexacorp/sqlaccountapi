export function useServiceEndpoint () {
    return {
        normalize(endpoint: string) {
            const apiUrl = import.meta.env.VITE_SQL_ACCOUNT_API_URL
                ? `${import.meta.env.VITE_SQL_ACCOUNT_API_URL}/${endpoint}`
                : `/${endpoint}`;
            return apiUrl
        }
    }
}
