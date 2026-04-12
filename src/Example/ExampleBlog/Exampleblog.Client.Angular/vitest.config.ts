// vitest.config.ts
import { defineConfig } from 'vitest/config'
export default defineConfig({
    test: {
        globals: true,
        environment: 'jsdom',
        includeTaskLocation: true,
        reporters: ["default", "junit"],
        outputFile: "test-results.xml",
        coverage: {
            provider: "istanbul",
            reporter: ["cobertura"],
        }
    }
})
