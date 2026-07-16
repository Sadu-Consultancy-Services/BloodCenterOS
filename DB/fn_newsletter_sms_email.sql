-- ============================================================================
-- Stored Procedures: NewsletterSubscription, SmsTemplateMaster, EmailTemplateMaster
-- ============================================================================

-- ── NewsletterSubscription ──
CREATE OR REPLACE FUNCTION fn_newsletter_create(
    p_center_id BIGINT, p_email VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO NewsletterSubscription (CenterId, Email, SubscribedAt, IsActive)
    VALUES (p_center_id, p_email, NOW(), TRUE)
    RETURNING SubscriptionId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_newsletter_update(p_subscription_id BIGINT, p_email VARCHAR, p_is_active BOOLEAN) RETURNS VOID AS $$
BEGIN
    UPDATE NewsletterSubscription SET
        Email = COALESCE(p_email, Email),
        IsActive = COALESCE(p_is_active, IsActive)
    WHERE SubscriptionId = p_subscription_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_newsletter_get_by_id(p_subscription_id BIGINT)
RETURNS TABLE(SubscriptionId BIGINT, CenterId BIGINT, Email VARCHAR, SubscribedAt TIMESTAMP, IsActive BOOLEAN) AS $$
BEGIN
    RETURN QUERY SELECT s.SubscriptionId, s.CenterId, s.Email, s.SubscribedAt, s.IsActive
    FROM NewsletterSubscription s WHERE s.SubscriptionId = p_subscription_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_newsletter_get_by_center(p_center_id BIGINT)
RETURNS TABLE(SubscriptionId BIGINT, CenterId BIGINT, Email VARCHAR, SubscribedAt TIMESTAMP, IsActive BOOLEAN) AS $$
BEGIN
    RETURN QUERY SELECT s.SubscriptionId, s.CenterId, s.Email, s.SubscribedAt, s.IsActive
    FROM NewsletterSubscription s WHERE s.CenterId = p_center_id ORDER BY s.SubscribedAt DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_newsletter_toggle_active(p_subscription_id BIGINT) RETURNS VOID AS $$
BEGIN
    UPDATE NewsletterSubscription SET IsActive = NOT IsActive WHERE SubscriptionId = p_subscription_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_newsletter_delete(p_subscription_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM NewsletterSubscription WHERE SubscriptionId = p_subscription_id;
END;
$$ LANGUAGE plpgsql;

-- ── SmsTemplateMaster ──
CREATE OR REPLACE FUNCTION fn_sms_template_create(
    p_center_id BIGINT, p_code VARCHAR, p_text VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO SmsTemplateMaster (CenterId, TemplateCode, TemplateText, CreatedAt)
    VALUES (p_center_id, p_code, p_text, NOW())
    RETURNING SmsTemplateId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_sms_template_update(
    p_template_id BIGINT, p_code VARCHAR, p_text VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE SmsTemplateMaster SET
        TemplateCode = COALESCE(p_code, TemplateCode),
        TemplateText = COALESCE(p_text, TemplateText)
    WHERE SmsTemplateId = p_template_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_sms_template_get_by_id(p_template_id BIGINT)
RETURNS TABLE(SmsTemplateId BIGINT, CenterId BIGINT, TemplateCode VARCHAR, TemplateText VARCHAR, CreatedAt TIMESTAMP) AS $$
BEGIN
    RETURN QUERY SELECT t.SmsTemplateId, t.CenterId, t.TemplateCode, t.TemplateText, t.CreatedAt
    FROM SmsTemplateMaster t WHERE t.SmsTemplateId = p_template_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_sms_template_get_by_center(p_center_id BIGINT)
RETURNS TABLE(SmsTemplateId BIGINT, CenterId BIGINT, TemplateCode VARCHAR, TemplateText VARCHAR, CreatedAt TIMESTAMP) AS $$
BEGIN
    RETURN QUERY SELECT t.SmsTemplateId, t.CenterId, t.TemplateCode, t.TemplateText, t.CreatedAt
    FROM SmsTemplateMaster t WHERE t.CenterId = p_center_id ORDER BY t.TemplateCode;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_sms_template_delete(p_template_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM SmsTemplateMaster WHERE SmsTemplateId = p_template_id;
END;
$$ LANGUAGE plpgsql;

-- ── EmailTemplateMaster ──
CREATE OR REPLACE FUNCTION fn_email_template_create(
    p_center_id BIGINT, p_code VARCHAR, p_subject VARCHAR, p_body TEXT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO EmailTemplateMaster (CenterId, TemplateCode, Subject, BodyHtml, CreatedAt)
    VALUES (p_center_id, p_code, p_subject, p_body, NOW())
    RETURNING EmailTemplateId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_email_template_update(
    p_template_id BIGINT, p_code VARCHAR, p_subject VARCHAR, p_body TEXT
) RETURNS VOID AS $$
BEGIN
    UPDATE EmailTemplateMaster SET
        TemplateCode = COALESCE(p_code, TemplateCode),
        Subject = COALESCE(p_subject, Subject),
        BodyHtml = COALESCE(p_body, BodyHtml)
    WHERE EmailTemplateId = p_template_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_email_template_get_by_id(p_template_id BIGINT)
RETURNS TABLE(EmailTemplateId BIGINT, CenterId BIGINT, TemplateCode VARCHAR, Subject VARCHAR, BodyHtml TEXT, CreatedAt TIMESTAMP) AS $$
BEGIN
    RETURN QUERY SELECT t.EmailTemplateId, t.CenterId, t.TemplateCode, t.Subject, t.BodyHtml, t.CreatedAt
    FROM EmailTemplateMaster t WHERE t.EmailTemplateId = p_template_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_email_template_get_by_center(p_center_id BIGINT)
RETURNS TABLE(EmailTemplateId BIGINT, CenterId BIGINT, TemplateCode VARCHAR, Subject VARCHAR, BodyHtml TEXT, CreatedAt TIMESTAMP) AS $$
BEGIN
    RETURN QUERY SELECT t.EmailTemplateId, t.CenterId, t.TemplateCode, t.Subject, t.BodyHtml, t.CreatedAt
    FROM EmailTemplateMaster t WHERE t.CenterId = p_center_id ORDER BY t.TemplateCode;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_email_template_delete(p_template_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM EmailTemplateMaster WHERE EmailTemplateId = p_template_id;
END;
$$ LANGUAGE plpgsql;
