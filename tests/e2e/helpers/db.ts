import pg from 'pg';

/** Rows in resource_holds for a trip (the table belongs to Resource Management, Student B). */
export async function countResourceHolds(tripId: string): Promise<number> {
  const url = process.env.E2E_DATABASE_URL;
  if (!url) throw new Error('Set E2E_DATABASE_URL to count resource_holds.');
  const client = new pg.Client({ connectionString: url });
  await client.connect();
  try {
    const result = await client.query('SELECT count(*)::int AS n FROM resource_holds WHERE trip_request_id = $1', [tripId]);
    return result.rows[0].n as number;
  } finally {
    await client.end();
  }
}
