CREATE OR REPLACE FUNCTION fn_billing_get_by_center(p_center_id BIGINT)
RETURNS TABLE(BillingTransactionId BIGINT, CenterId BIGINT, InvoiceNumber VARCHAR,
    PatientId BIGINT, TotalAmount DECIMAL, TaxAmount DECIMAL, Discount DECIMAL,
    PaymentStatus VARCHAR, PaymentMode VARCHAR, InvoiceDate TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT b.BillingTransactionId, b.CenterId, b.InvoiceNumber,
        b.PatientId, b.TotalAmount, b.TaxAmount, b.Discount,
        b.PaymentStatus, b.PaymentMode, b.InvoiceDate, b.CreatedAt
    FROM BillingTransaction b
    WHERE b.CenterId = p_center_id
    ORDER BY b.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;
